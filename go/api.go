package main

import (
	"context"
	"database/sql"
	"encoding/json"
	"errors"
	"fmt"
	"log"
	"net/http"
	"os"
	"regexp"
	"strconv"
	"strings"
	"time"

	"github.com/go-redis/redis/v8"
	"github.com/golang-jwt/jwt/v5"
	"github.com/google/uuid"
	"github.com/gorilla/websocket"
	amqp "github.com/rabbitmq/amqp091-go"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"
)

var (
	clients   = make(map[*websocket.Conn]*ManagerSession)
	broadcast = make(chan TaskMessage)
	upgrader  = websocket.Upgrader{
		CheckOrigin: func(r *http.Request) bool { return true },
	}
	redisClient *redis.Client
	db          *gorm.DB
)

func logInfo(format string, args ...any) {
	log.Printf("[INFO] "+format, args...)
}

func logError(format string, args ...any) {
	log.Printf("[ERROR] "+format, args...)
}

func logDebug(format string, args ...any) {
	log.Printf("[DEBUG] "+format, args...)
}

func logWarn(format string, args ...any) {
	log.Printf("[WARN] "+format, args...)
}

type ManagerSession struct {
	Conn      *websocket.Conn
	ManagerID uuid.UUID
	TeamID    uuid.UUID
	SprintID  uuid.UUID
}

type TaskMessage struct {
	Action   string    `json:"action"`
	TaskID   uuid.UUID `json:"task_id"`
	TaskData *TaskInfo `json:"task_data,omitempty"`
	Token    string    `json:"token,omitempty"`
	Error    string    `json:"error,omitempty"`
}

type TaskInfo struct {
	ID            uuid.UUID     `json:"id"`
	Name          string        `json:"name"`
	TotalDuration time.Duration `json:"total_duration"`
	InProgress    bool          `json:"in_progress"`
	DeveloperName string        `json:"developer_name,omitempty"`
	Status        string        `json:"status,omitempty"`
	IsStarted     bool          `json:"is_started"`
	IsPaused      bool          `json:"is_paused"`
	IsStopped     bool          `json:"is_stopped"`
	StartTime     *time.Time    `json:"start_time,omitempty"`
	UpdateTime    time.Time     `json:"update_time,omitempty"`
}

type RabbitMQConfig struct {
	Host     string
	Port     string
	User     string
	Password string
}

func initRedis() {
	redisHost := getEnv("REDIS_HOST", "redis")
	redisPort := getEnv("REDIS_PORT", "6379")
	redisAddr := fmt.Sprintf("%s:%s", redisHost, redisPort)

	logInfo("Connecting to Redis at %s", redisAddr)

	redisClient = redis.NewClient(&redis.Options{
		Addr: redisAddr,
	})

	ctx := context.Background()
	_, err := redisClient.Ping(ctx).Result()
	if err != nil {
		logError("Failed to connect to Redis: %v", err)
		log.Fatalf("Redis connection failed")
	}
	logInfo("Successfully connected to Redis")
}

func initDatabase() {
	dbHost := getEnv("DB_HOST", "database")
	dbPort := getEnv("DB_PORT", "5432")
	dbName := getEnv("DB_DATABASE", "project")
	dbUser := getEnv("DB_USERNAME", "postgres")
	dbPassword := getEnv("DB_PASSWORD", "password")

	dsn := fmt.Sprintf("host=%s port=%s user=%s password=%s dbname=%s sslmode=disable",
		dbHost, dbPort, dbUser, dbPassword, dbName)

	logInfo("Connecting to database: %s@%s:%s/%s", dbUser, dbHost, dbPort, dbName)

	var err error
	db, err = gorm.Open(postgres.Open(dsn), &gorm.Config{
		Logger: logger.Default.LogMode(logger.Info),
	})
	if err != nil {
		logError("Failed to connect to database: %v", err)
		log.Fatalf("Database connection failed")
	}

	sqlDB, err := db.DB()
	if err != nil {
		logError("Failed to get underlying sql.DB: %v", err)
		log.Fatalf("Database configuration failed")
	}

	if err = sqlDB.Ping(); err != nil {
		logError("Failed to ping database: %v", err)
		log.Fatalf("Database ping failed")
	}

	logInfo("Successfully connected to database")
}

func validateJWT(tokenString string) (uuid.UUID, error) {
	logDebug("Validating JWT token")

	jwtSecret := getEnv("JWT_TOKEN", "")
	jwtIssuer := getEnv("JWT_ISSUER", "")
	jwtAudience := getEnv("JWT_AUDIENCE", "")

	token, err := jwt.Parse(tokenString, func(token *jwt.Token) (any, error) {
		if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
			logWarn("Unexpected signing method: %v", token.Header["alg"])
			return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
		}
		return []byte(jwtSecret), nil
	})

	if err != nil {
		logWarn("JWT parsing failed: %v", err)
		return uuid.Nil, err
	}

	if claims, ok := token.Claims.(jwt.MapClaims); ok && token.Valid {
		if claims["iss"] != jwtIssuer || claims["aud"] != jwtAudience {
			logWarn("Invalid token issuer or audience: iss=%v, aud=%v", claims["iss"], claims["aud"])
			return uuid.Nil, fmt.Errorf("invalid token claims")
		}

		role, ok := claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"].(string)
		if !ok || role != "manager" {
			logWarn("User is not a manager, role: %v", role)
			return uuid.Nil, fmt.Errorf("user is not a manager")
		}

		userIDStr, ok := claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"].(string)
		if !ok {
			logWarn("Invalid user ID in token claims")
			return uuid.Nil, fmt.Errorf("invalid user id in token")
		}

		userID, err := uuid.Parse(userIDStr)
		if err != nil {
			logWarn("Failed to parse user ID as UUID: %v", err)
			return uuid.Nil, fmt.Errorf("invalid user id format")
		}

		logInfo("JWT validated successfully for manager ID: %s", userID)
		return userID, nil
	}

	logWarn("Invalid token or claims")
	return uuid.Nil, fmt.Errorf("invalid token")
}

func checkTokenInRedis(managerID uuid.UUID, token string) bool {
	ctx := context.Background()
	redisKey := fmt.Sprintf("manager_token:%s", managerID.String())

	logDebug("Checking token in Redis for manager ID: %s", managerID)

	storedToken, err := redisClient.Get(ctx, redisKey).Result()
	if err != nil {
		if errors.Is(err, redis.Nil) {
			logWarn("Token not found in Redis for manager ID: %s", managerID)
		} else {
			logError("Redis error while checking token: %v", err)
		}
		return false
	}

	if storedToken == token {
		logInfo("Token validated successfully from Redis for manager ID: %s", managerID)
		return true
	}

	logWarn("Token mismatch in Redis for manager ID: %s", managerID)
	return false
}

func getRabbitMQConfig() RabbitMQConfig {
	return RabbitMQConfig{
		Host:     getEnv("RABBITMQ_HOST", "rabbitmq"),
		Port:     getEnv("RABBITMQ_PORT", "5672"),
		User:     getEnv("RABBITMQ_USER", "user"),
		Password: getEnv("RABBITMQ_PASS", "password"),
	}
}

func getEnv(key, defaultValue string) string {
	if value, exists := os.LookupEnv(key); exists {
		return value
	}
	return defaultValue
}

func getManagerTeamAndSprint(managerID uuid.UUID) (uuid.UUID, uuid.UUID, error) {
	logDebug("Getting team and sprint for manager ID: %s using procedure", managerID)

	rows, err := db.Raw("SELECT * FROM GetManagerTeamAndSprint(?)", managerID.String()).Rows()
	if err != nil {
		logError("Failed to call GetManagerTeamAndSprint procedure for manager %s: %v", managerID, err)
		return uuid.Nil, uuid.Nil, fmt.Errorf("failed to call GetManagerTeamAndSprint procedure: %v", err)
	}
	defer func(rows *sql.Rows) {
		err := rows.Close()
		if err != nil {
			logError("Error closing rows: %v", err)
		}
	}(rows)

	if !rows.Next() {
		logWarn("No team found for manager ID: %s", managerID)
		return uuid.Nil, uuid.Nil, fmt.Errorf("no team found for manager")
	}

	var teamIDStr, sprintIDStr string
	err = rows.Scan(&teamIDStr, &sprintIDStr)
	if err != nil {
		logError("Failed to scan team and sprint data: %v", err)
		return uuid.Nil, uuid.Nil, fmt.Errorf("failed to scan team and sprint data: %v", err)
	}

	teamID, err := uuid.Parse(teamIDStr)
	if err != nil {
		logError("Failed to parse team ID as UUID: %v", err)
		return uuid.Nil, uuid.Nil, fmt.Errorf("invalid team ID format: %v", err)
	}

	var sprintID uuid.UUID
	if sprintIDStr != "" {
		sprintID, err = uuid.Parse(sprintIDStr)
		if err != nil {
			logError("Failed to parse sprint ID as UUID: %v", err)
			return teamID, uuid.Nil, fmt.Errorf("invalid sprint ID format: %v", err)
		}
	} else {
		logWarn("No sprint found for manager's team %s", teamID)
		return teamID, uuid.Nil, nil
	}

	logInfo("Found team ID: %s, sprint ID: %s for manager ID: %s", teamID, sprintID, managerID)
	return teamID, sprintID, nil
}

func parsePostgresInterval(intervalStr string) (time.Duration, error) {
	if intervalStr == "00:00:00" {
		return 0, nil
	}

	var totalDuration time.Duration

	if strings.Contains(intervalStr, "day") {
		parts := strings.Split(intervalStr, " ")
		for i, part := range parts {
			if part == "day" && i > 0 {
				days, err := strconv.Atoi(parts[i-1])
				if err == nil {
					totalDuration += time.Duration(days) * 24 * time.Hour
				}
				intervalStr = strings.Join(parts[i+1:], " ")
				break
			}
		}
	}

	if strings.TrimSpace(intervalStr) != "" {
		// Regex HH:MM:SS.ssssss
		re := regexp.MustCompile(`(\d+):(\d+):(\d+)\.?(\d*)`)
		matches := re.FindStringSubmatch(intervalStr)

		if len(matches) >= 4 {
			hours, _ := strconv.Atoi(matches[1])
			minutes, _ := strconv.Atoi(matches[2])
			seconds, _ := strconv.Atoi(matches[3])

			totalDuration += time.Duration(hours) * time.Hour
			totalDuration += time.Duration(minutes) * time.Minute
			totalDuration += time.Duration(seconds) * time.Second

			// micro/nanoseconds
			if len(matches) > 4 && matches[4] != "" {
				microStr := matches[4]
				for len(microStr) < 6 {
					microStr += "0"
				}
				microseconds, _ := strconv.Atoi(microStr[:6])
				totalDuration += time.Duration(microseconds) * time.Microsecond
			}
		}
	}

	return totalDuration, nil
}

func getManagerTasks(managerID uuid.UUID) ([]TaskInfo, error) {
	logInfo("Getting tasks for manager ID: %s using optimized procedure", managerID)

	rows, err := db.Raw("SELECT * FROM GetManagerTasksWithDetails(?)", managerID.String()).Rows()
	if err != nil {
		logError("Failed to call GetManagerTasksWithDetails procedure for manager %s: %v", managerID, err)
		return nil, fmt.Errorf("failed to call GetManagerTasksWithDetails procedure: %v", err)
	}
	defer func(rows *sql.Rows) {
		err := rows.Close()
		if err != nil {
			logError("Error closing rows: %v", err)
		}
	}(rows)

	var taskInfos []TaskInfo

	for rows.Next() {
		var taskID, sprintID, teamID, taskName, totalDurationStr, currentStatus, developerName string
		var isStarted, isPaused, isStopped bool
		var startTime, updateTime *time.Time

		err := rows.Scan(&taskID, &taskName, &totalDurationStr, &isStarted, &isPaused, &isStopped,
			&currentStatus, &developerName, &startTime, &updateTime, &sprintID, &teamID)
		if err != nil {
			logError("Failed to scan task row: %v", err)
			continue
		}

		taskUUID, err := uuid.Parse(taskID)
		if err != nil {
			logError("Failed to parse task ID as UUID: %v", err)
			continue
		}

		totalDuration, err := parsePostgresInterval(totalDurationStr)
		if err != nil {
			logWarn("Failed to parse duration for task %s: %v, using 0", taskID, err)
			totalDuration = 0
		}

		taskInfo := TaskInfo{
			ID:            taskUUID,
			Name:          taskName,
			TotalDuration: totalDuration,
			InProgress:    isStarted,
			DeveloperName: developerName,
			Status:        currentStatus,
			IsStarted:     isStarted,
			IsPaused:      isPaused,
			IsStopped:     isStopped,
			StartTime:     startTime,
			UpdateTime:    *updateTime,
		}

		taskInfos = append(taskInfos, taskInfo)

		logDebug("Task %s (%s) - Duration: %v, IsStarted: %t, IsPaused: %t, IsStopped: %t, Developer: %s, Status: %s",
			taskUUID, taskName, totalDuration, isStarted, isPaused, isStopped, developerName, currentStatus)
	}

	if err = rows.Err(); err != nil {
		logError("Error iterating over task rows: %v", err)
		return nil, fmt.Errorf("error iterating over task rows: %v", err)
	}

	logInfo("Successfully retrieved %d tasks for manager %s", len(taskInfos), managerID)
	return taskInfos, nil
}

func checkRabbitMQConnection() bool {
	config := getRabbitMQConfig()
	connURL := fmt.Sprintf("amqp://%s:%s@%s:%s/",
		config.User, config.Password, config.Host, config.Port)

	conn, err := amqp.Dial(connURL)
	if err != nil {
		log.Printf("Error connecting to RabbitMQ: %v", err)
		return false
	}
	defer func(conn *amqp.Connection) {
		err := conn.Close()
		if err != nil {
			log.Printf("Error closing RabbitMQ connection: %v", err)
		}
	}(conn)

	return true
}

func healthHandler(w http.ResponseWriter, _ *http.Request) {
	rabbitmqOK := checkRabbitMQConnection()

	w.Header().Set("Content-Type", "application/json")

	if !rabbitmqOK {
		w.WriteHeader(http.StatusServiceUnavailable)
		err := json.NewEncoder(w).Encode(map[string]string{
			"status":   "unhealthy",
			"rabbitmq": "disconnected",
		})
		if err != nil {
			return
		}
		return
	}

	w.WriteHeader(http.StatusOK)
	err := json.NewEncoder(w).Encode(map[string]string{
		"status":   "healthy",
		"rabbitmq": "connected",
	})
	if err != nil {
		return
	}
}

func handleConnections(w http.ResponseWriter, r *http.Request) {
	logInfo("New WebSocket connection request from %s", r.RemoteAddr)

	ws, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		logError("WebSocket upgrade failed: %v", err)
		return
	}
	defer func(ws *websocket.Conn) {
		err := ws.Close()
		if err != nil {
			logError("Error closing WebSocket connection: %v", err)
		}
	}(ws)

	logInfo("WebSocket connection established, waiting for authentication")

	_, msgBytes, err := ws.ReadMessage()
	if err != nil {
		logError("Failed to read authentication message: %v", err)
		return
	}

	var authMsg TaskMessage
	err = json.Unmarshal(msgBytes, &authMsg)
	if err != nil {
		logError("Failed to parse authentication message: %v", err)
		err := ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Invalid message format"}`))
		if err != nil {
			logError("Failed to send auth error message: %v", err)
			return
		}
		return
	}

	if authMsg.Action != "authenticate" || authMsg.Token == "" {
		logWarn("Invalid authentication message from %s", r.RemoteAddr)
		err := ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Missing authentication"}`))
		if err != nil {
			logError("Failed to send auth error message: %v", err)
			return
		}
		return
	}

	logDebug("Received authentication message with token")

	managerID, err := validateJWT(authMsg.Token)
	if err != nil {
		logError("JWT validation failed for WebSocket connection: %v", err)
		err := ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Invalid token"}`))
		if err != nil {
			logError("Failed to send auth error message: %v", err)
			return
		}
		return
	}

	logInfo("JWT validated for manager ID: %s", managerID)

	if !checkTokenInRedis(managerID, authMsg.Token) {
		logWarn("Token validation failed in Redis for manager %s", managerID)
		err := ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Token not found or expired"}`))
		if err != nil {
			logError("Failed to send auth error message: %v", err)
			return
		}
		return
	}

	logInfo("Token validated in Redis for manager %s", managerID)

	teamID, sprintID, err := getManagerTeamAndSprint(managerID)
	if err != nil {
		logError("Failed to get manager data for %s: %v", managerID, err)
		err := ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Manager data not found"}`))
		if err != nil {
			logError("Failed to send auth error message: %v", err)
			return
		}
		return
	}

	logInfo("Manager %s associated with team %s and sprint %s", managerID, teamID, sprintID)

	authSuccessMsg := TaskMessage{Action: "auth_success"}
	msgBytes, _ = json.Marshal(authSuccessMsg)
	err = ws.WriteMessage(websocket.TextMessage, msgBytes)
	if err != nil {
		logError("Failed to send auth success message: %v", err)
		return
	}

	logInfo("Authentication successful for manager %s", managerID)

	session := &ManagerSession{
		Conn:      ws,
		ManagerID: managerID,
		TeamID:    teamID,
		SprintID:  sprintID,
	}

	clients[ws] = session
	logInfo("Added manager %s to active WebSocket clients (total: %d)", managerID, len(clients))

	tasks, err := getManagerTasks(managerID)
	if err != nil {
		logError("Failed to get initial tasks for manager %s: %v", managerID, err)
	} else {
		logInfo("Sending %d initial tasks to manager %s", len(tasks), managerID)

		for _, task := range tasks {
			taskMsg := TaskMessage{
				Action:   "task_update",
				TaskID:   task.ID,
				TaskData: &task,
			}

			msgBytes, _ := json.Marshal(taskMsg)
			err := ws.WriteMessage(websocket.TextMessage, msgBytes)
			if err != nil {
				logError("Failed to send task %s to manager %s: %v", task.ID, managerID, err)
			} else {
				logDebug("Sent task %s (%s) to manager %s", task.ID, task.Name, managerID)
			}
		}

		initialMessage := TaskMessage{
			Action: "initial_tasks",
		}
		msgBytes, _ := json.Marshal(initialMessage)
		err = ws.WriteMessage(websocket.TextMessage, msgBytes)
		if err != nil {
			logError("Failed to send initial_tasks message to manager %s: %v", managerID, err)
		} else {
			logInfo("Successfully sent all initial tasks to manager %s", managerID)
		}
	}

	logInfo("WebSocket connection for manager %s ready for messages", managerID)
	for {
		_, _, err := ws.ReadMessage()
		if err != nil {
			logInfo("WebSocket connection closed for manager %s: %v", managerID, err)
			delete(clients, ws)
			logInfo("Removed manager %s from active clients (remaining: %d)", managerID, len(clients))
			break
		}
	}
}

func handleMessages() {
	logInfo("Started message handling goroutine for WebSocket broadcasts")
	for {
		msg := <-broadcast
		logInfo("Processing broadcast message - Action: %s, TaskID: %s", msg.Action, msg.TaskID)

		clientCount := 0
		sentCount := 0
		for conn, session := range clients {
			clientCount++

			if msg.TaskData != nil {
				taskBelongsToManager := checkTaskBelongsToManager(msg.TaskID, session.ManagerID)
				if !taskBelongsToManager {
					logDebug("Task %s does not belong to manager %s, skipping",
						msg.TaskID, session.ManagerID)
					continue
				}
				logDebug("Task %s belongs to manager %s, sending update",
					msg.TaskID, session.ManagerID)
			}

			msgBytes, err := json.Marshal(msg)
			if err != nil {
				logError("Error marshaling message for manager %s: %v", session.ManagerID, err)
				continue
			}

			err = conn.WriteMessage(websocket.TextMessage, msgBytes)
			if err != nil {
				logError("Error sending message to manager %s: %v", session.ManagerID, err)
				err := conn.Close()
				if err != nil {
					logError("Error closing WebSocket connection for manager %s: %v", session.ManagerID, err)
					return
				}
				delete(clients, conn)
			} else {
				sentCount++
				logDebug("Successfully sent message to manager %s", session.ManagerID)
			}
		}

		logInfo("Broadcast completed - Checked %d clients, sent to %d clients", clientCount, sentCount)
	}
}

func getTaskInfo(taskID uuid.UUID) (*TaskInfo, error) {
	logDebug("Getting task info for task ID: %s using optimized procedure", taskID)

	rows, err := db.Raw("SELECT * FROM GetSingleTaskDetails(?)", taskID.String()).Rows()
	if err != nil {
		logError("Failed to call GetSingleTaskDetails procedure for task %s: %v", taskID, err)
		return nil, fmt.Errorf("failed to call GetSingleTaskDetails procedure: %v", err)
	}
	defer func(rows *sql.Rows) {
		err := rows.Close()
		if err != nil {
			logError("Error closing rows: %v", err)
		}
	}(rows)

	if !rows.Next() {
		logWarn("Task %s not found", taskID)
		return nil, fmt.Errorf("task not found")
	}

	var taskIDStr, sprintID, teamID, managerID, taskName, totalDurationStr, currentStatus, developerName string
	var isStarted, isPaused, isStopped bool
	var startTime, updateTime *time.Time

	err = rows.Scan(&taskIDStr, &taskName, &totalDurationStr, &isStarted, &isPaused, &isStopped,
		&currentStatus, &developerName, &startTime, &updateTime, &sprintID, &teamID, &managerID)
	if err != nil {
		logError("Failed to scan task row: %v", err)
		return nil, fmt.Errorf("failed to scan task row: %v", err)
	}

	taskUUID, err := uuid.Parse(taskIDStr)
	if err != nil {
		logError("Failed to parse task ID as UUID: %v", err)
		return nil, fmt.Errorf("invalid task ID format: %v", err)
	}

	totalDuration, err := parsePostgresInterval(totalDurationStr)
	if err != nil {
		logWarn("Failed to parse duration for task %s: %v, using 0", taskID, err)
		totalDuration = 0
	}

	taskInfo := TaskInfo{
		ID:            taskUUID,
		Name:          taskName,
		TotalDuration: totalDuration,
		InProgress:    isStarted,
		DeveloperName: developerName,
		Status:        currentStatus,
		IsStarted:     isStarted,
		IsPaused:      isPaused,
		IsStopped:     isStopped,
		StartTime:     startTime,
		UpdateTime:    *updateTime,
	}

	logInfo("Retrieved task info: %s (%s) - Duration: %v, Status: %s, Developer: %s",
		taskID, taskName, totalDuration, currentStatus, developerName)
	return &taskInfo, nil
}

func checkTaskBelongsToManager(taskID, managerID uuid.UUID) bool {
	logDebug("Checking if task %s belongs to manager %s", taskID, managerID)

	var belongs bool
	err := db.Raw("SELECT CheckTaskBelongsToManager(?, ?)", taskID.String(), managerID.String()).Row().Scan(&belongs)
	if err != nil {
		logError("Failed to check if task belongs to manager: %v", err)
		return false
	}

	logDebug("Task %s belongs to manager %s: %t", taskID, managerID, belongs)
	return belongs
}

func consumeRabbitMQ() {
	logInfo("Starting RabbitMQ consumer")
	var conn *amqp.Connection
	var err error

	config := getRabbitMQConfig()
	connURL := fmt.Sprintf("amqp://%s:%s@%s:%s/",
		config.User, config.Password, config.Host, config.Port)

	logInfo("Connecting to RabbitMQ at %s:%s", config.Host, config.Port)
	conn, err = amqp.Dial(connURL)

	if err != nil {
		logError("Failed to connect to RabbitMQ: %v", err)
		log.Fatalf("Failed to connect to RabbitMQ after 5 attempts: %v", err)
	}
	defer func(conn *amqp.Connection) {
		err := conn.Close()
		if err != nil {
			logError("Error closing RabbitMQ connection: %v", err)
		}
	}(conn)
	logInfo("Successfully connected to RabbitMQ")

	ch, err := conn.Channel()
	if err != nil {
		logError("Error opening RabbitMQ channel: %v", err)
		log.Fatalf("Error opening channel: %v", err)
	}
	defer func(ch *amqp.Channel) {
		err := ch.Close()
		if err != nil {
			logError("Error closing RabbitMQ channel: %v", err)
		}
	}(ch)
	logInfo("RabbitMQ channel opened successfully")

	q, err := ch.QueueDeclare(
		"task_queue",
		true,
		false,
		false,
		false,
		nil,
	)
	if err != nil {
		logError("Error declaring queue: %v", err)
		log.Fatalf("Error declaring queue: %v", err)
	}
	logInfo("Queue 'task_queue' declared successfully")

	messages, err := ch.Consume(
		q.Name,
		"",
		true,
		false,
		false,
		false,
		nil,
	)
	if err != nil {
		logError("Error consuming messages: %v", err)
		log.Fatalf("Error consuming messages: %v", err)
	}
	logInfo("Started consuming messages from task_queue")

	for d := range messages {
		logInfo("Received message from RabbitMQ queue: %s", string(d.Body))

		var rabbitMsg map[string]any
		if err := json.Unmarshal(d.Body, &rabbitMsg); err != nil {
			logError("Failed to parse RabbitMQ message: %v", err)
			continue
		}

		taskIDStr, ok := rabbitMsg["task_id"].(string)
		if !ok {
			logError("Invalid task_id in RabbitMQ message: %v", rabbitMsg)
			continue
		}

		taskID, err := uuid.Parse(taskIDStr)
		if err != nil {
			logError("Failed to parse task_id as UUID: %v", err)
			continue
		}

		action, ok := rabbitMsg["action"].(string)
		if !ok {
			logError("Invalid action in RabbitMQ message: %v", rabbitMsg)
			continue
		}

		logInfo("Processing RabbitMQ message - TaskID: %s, Action: %s", taskID, action)

		time.Sleep(200 * time.Millisecond)

		taskInfo, err := getTaskInfo(taskID)
		if err != nil {
			logError("Failed to get task info for task %s: %v", taskID, err)
			continue
		}

		taskMessage := TaskMessage{
			Action:   action,
			TaskID:   taskID,
			TaskData: taskInfo,
		}

		logInfo("Broadcasting task update - TaskID: %s, Action: %s, Status: %s",
			taskID, action, taskInfo.Status)
		broadcast <- taskMessage
	}
}

func main() {
	logInfo("=== Starting Go WebSocket Service for Manager Dashboard ===")

	logInfo("Initializing Redis connection...")
	initRedis()

	logInfo("Initializing database connection...")
	initDatabase()

	logInfo("Starting RabbitMQ consumer goroutine...")
	go consumeRabbitMQ()

	logInfo("Setting up HTTP handlers...")
	http.HandleFunc("/ws", handleConnections)
	http.HandleFunc("/health", healthHandler)

	logInfo("Starting message broadcaster goroutine...")
	go handleMessages()

	logInfo("=== Go WebSocket server ready on port :8080 ===")
	log.Println("Go server with WebSocket on port :8080")
	if err := http.ListenAndServe("0.0.0.0:8080", nil); err != nil {
		logError("Failed to start HTTP server: %v", err)
		log.Fatal("ListenAndServe: ", err)
	}
}
