package main

import (
	"context"
	"encoding/json"
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
	"github.com/streadway/amqp"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"

	"generator/model"
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
		if err == redis.Nil {
			logWarn("Token not found in Redis for manager ID: %s", managerID)
		} else {
			logError("Redis error while checking token: %v", err)
		}
		return false
	}

	isValid := storedToken == token
	if isValid {
		logInfo("Token validated successfully from Redis for manager ID: %s", managerID)
	} else {
		logWarn("Token mismatch in Redis for manager ID: %s", managerID)
	}

	return isValid
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
	logDebug("Getting team and sprint for manager ID: %s", managerID)

	var team model.Team
	err := db.Where("\"ManagerId\" = ?", managerID.String()).First(&team).Error
	if err != nil {
		if err == gorm.ErrRecordNotFound {
			logWarn("No team found for manager ID: %s", managerID)
		} else {
			logError("Database error getting manager's team: %v", err)
		}
		return uuid.Nil, uuid.Nil, fmt.Errorf("failed to get manager's team: %v", err)
	}

	teamID, err := uuid.Parse(team.ID)
	if err != nil {
		logError("Failed to parse team ID as UUID: %v", err)
		return uuid.Nil, uuid.Nil, fmt.Errorf("invalid team ID format: %v", err)
	}

	var sprint model.Sprint
	err = db.Where("\"TeamId\" = ?", team.ID).
		Order("\"StartDate\" DESC").
		First(&sprint).Error
	if err != nil {
		if err == gorm.ErrRecordNotFound {
			logWarn("No sprint found for manager's team %s", teamID)
			return teamID, uuid.Nil, nil
		} else {
			logError("Database error getting latest sprint: %v", err)
			return teamID, uuid.Nil, fmt.Errorf("failed to get latest sprint: %v", err)
		}
	}

	sprintID, err := uuid.Parse(sprint.ID)
	if err != nil {
		logError("Failed to parse sprint ID as UUID: %v", err)
		return teamID, uuid.Nil, fmt.Errorf("invalid sprint ID format: %v", err)
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

func getManagerTasks(sprintID uuid.UUID) ([]TaskInfo, error) {
	logInfo("Getting tasks for sprint ID: %s using PostgreSQL procedure", sprintID)

	rows, err := db.Raw("SELECT * FROM GetTaskDurations(?)", sprintID.String()).Rows()
	if err != nil {
		logError("Failed to call GetTaskDurations procedure for sprint %s: %v", sprintID, err)
		return nil, fmt.Errorf("failed to call GetTaskDurations procedure: %v", err)
	}
	defer rows.Close()

	var taskInfos []TaskInfo

	for rows.Next() {
		var taskID string
		var taskName string
		var totalDurationStr string
		var inProgress bool

		err := rows.Scan(&taskID, &taskName, &totalDurationStr, &inProgress)
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

		var developer model.User
		var developerName string
		var task model.Task

		err = db.Where("\"Id\" = ?", taskID).First(&task).Error
		if err == nil {
			if task.DeveloperID != "" {
				err = db.Where("\"Id\" = ?", task.DeveloperID).First(&developer).Error
				if err == nil {
					developerName = developer.Username
				} else {
					developerName = "Unknown"
				}
			} else {
				developerName = "Unassigned"
			}
		}

		var status model.TaskStatus
		var statusName string
		if task.TaskStatusID != 0 {
			err = db.Where("\"Id\" = ?", task.TaskStatusID).First(&status).Error
			if err == nil {
				statusName = status.Name
			}
		}

		var history model.TaskHistory
		var updateTime time.Time
		err = db.Where("\"TaskId\" = ?", taskID).
			Order("\"ChangeDate\" DESC").
			First(&history).Error
		if err == nil {
			updateTime = history.ChangeDate
		} else {
			updateTime = time.Now()
		}

		var startTime *time.Time
		if inProgress {
			startTime = getTaskStartTime(taskUUID)
		}

		taskInfo := TaskInfo{
			ID:            taskUUID,
			Name:          taskName,
			TotalDuration: totalDuration,
			InProgress:    inProgress,
			DeveloperName: developerName,
			Status:        statusName,
			StartTime:     startTime,
			UpdateTime:    updateTime,
		}

		taskInfos = append(taskInfos, taskInfo)

		logDebug("Task %s (%s) - Duration: %v, InProgress: %t, Developer: %s, Status: %s",
			taskUUID, taskName, totalDuration, inProgress, developerName, statusName)
	}

	if err = rows.Err(); err != nil {
		logError("Error iterating over task rows: %v", err)
		return nil, fmt.Errorf("error iterating over task rows: %v", err)
	}

	logInfo("Successfully retrieved %d tasks from GetTaskDurations procedure for sprint %s", len(taskInfos), sprintID)
	return taskInfos, nil
}

func getTaskStartTime(taskID uuid.UUID) *time.Time {
	logDebug("Getting start time for task ID: %s", taskID)

	var history model.TaskHistory
	err := db.Joins("JOIN \"TaskStatuses\" ON \"TaskHistories\".\"TaskStatusId\" = \"TaskStatuses\".\"Id\"").
		Where("\"TaskHistories\".\"TaskId\" = ? AND \"TaskStatuses\".\"Name\" = ?", taskID.String(), "Started").
		Order("\"ChangeDate\" DESC").
		First(&history).Error

	if err != nil {
		logWarn("No start time found for task %s: %v", taskID, err)
		return nil
	}

	logDebug("Found start time for task %s: %v", taskID, history.ChangeDate)
	return &history.ChangeDate
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
	defer conn.Close()

	return true
}

func healthHandler(w http.ResponseWriter, _ *http.Request) {
	rabbitmqOK := checkRabbitMQConnection()

	w.Header().Set("Content-Type", "application/json")

	if !rabbitmqOK {
		w.WriteHeader(http.StatusServiceUnavailable)
		json.NewEncoder(w).Encode(map[string]string{
			"status":   "unhealthy",
			"rabbitmq": "disconnected",
		})
		return
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{
		"status":   "healthy",
		"rabbitmq": "connected",
	})
}

func handleConnections(w http.ResponseWriter, r *http.Request) {
	logInfo("New WebSocket connection request from %s", r.RemoteAddr)

	ws, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		logError("WebSocket upgrade failed: %v", err)
		return
	}
	defer ws.Close()

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
		ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Invalid message format"}`))
		return
	}

	if authMsg.Action != "authenticate" || authMsg.Token == "" {
		logWarn("Invalid authentication message from %s", r.RemoteAddr)
		ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Missing authentication"}`))
		return
	}

	logDebug("Received authentication message with token")

	managerID, err := validateJWT(authMsg.Token)
	if err != nil {
		logError("JWT validation failed for WebSocket connection: %v", err)
		ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Invalid token"}`))
		return
	}

	logInfo("JWT validated for manager ID: %s", managerID)

	if !checkTokenInRedis(managerID, authMsg.Token) {
		logWarn("Token validation failed in Redis for manager %s", managerID)
		ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Token not found or expired"}`))
		return
	}

	logInfo("Token validated in Redis for manager %s", managerID)

	teamID, sprintID, err := getManagerTeamAndSprint(managerID)
	if err != nil {
		logError("Failed to get manager data for %s: %v", managerID, err)
		ws.WriteMessage(websocket.TextMessage, []byte(`{"action":"auth_error","error":"Manager data not found"}`))
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

	tasks, err := getManagerTasks(sprintID)
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
				taskBelongsToManager := checkTaskBelongsToSprint(msg.TaskID, session.SprintID)
				if !taskBelongsToManager {
					logDebug("Task %s does not belong to manager %s's sprint %s, skipping",
						msg.TaskID, session.ManagerID, session.SprintID)
					continue
				}
				logDebug("Task %s belongs to manager %s's sprint %s, sending update",
					msg.TaskID, session.ManagerID, session.SprintID)
			}

			msgBytes, err := json.Marshal(msg)
			if err != nil {
				logError("Error marshaling message for manager %s: %v", session.ManagerID, err)
				continue
			}

			err = conn.WriteMessage(websocket.TextMessage, msgBytes)
			if err != nil {
				logError("Error sending message to manager %s: %v", session.ManagerID, err)
				conn.Close()
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
	logDebug("Getting task info for task ID: %s", taskID)

	var task model.Task
	err := db.Where("\"Id\" = ?", taskID.String()).First(&task).Error
	if err != nil {
		logError("Failed to get task info for task %s: %v", taskID, err)
		return nil, fmt.Errorf("failed to get task info: %v", err)
	}
	rows, err := db.Raw("SELECT * FROM GetTaskDurations(?) WHERE task_id = ?", task.SprintID, taskID.String()).Rows()
	if err != nil {
		logError("Failed to call GetTaskDurations for task %s: %v", taskID, err)
		return nil, fmt.Errorf("failed to call GetTaskDurations: %v", err)
	}
	defer rows.Close()

	var totalDuration time.Duration
	var inProgress bool

	if rows.Next() {
		var taskIDFromProc, taskName, totalDurationStr string
		err := rows.Scan(&taskIDFromProc, &taskName, &totalDurationStr, &inProgress)
		if err != nil {
			logError("Failed to scan task duration row: %v", err)
		} else {
			totalDuration, err = parsePostgresInterval(totalDurationStr)
			if err != nil {
				logWarn("Failed to parse duration for task %s: %v, using 0", taskID, err)
				totalDuration = 0
			}
		}
	}

	var developerName string
	if task.DeveloperID != "" {
		var developer model.User
		err := db.Where("\"Id\" = ?", task.DeveloperID).First(&developer).Error
		if err == nil {
			developerName = developer.Username
		} else {
			developerName = "Unknown"
		}
	} else {
		developerName = "Unassigned"
	}

	var statusName string
	var status model.TaskStatus
	err = db.Where("\"Id\" = ?", task.TaskStatusID).First(&status).Error
	if err == nil {
		statusName = status.Name
	}

	var history model.TaskHistory
	var updateTime time.Time
	err = db.Where("\"TaskId\" = ?", task.ID).
		Order("\"ChangeDate\" DESC").
		First(&history).Error
	if err == nil {
		updateTime = history.ChangeDate
	} else {
		updateTime = time.Now()
	}

	taskInfo := TaskInfo{
		ID:            taskID,
		Name:          task.Name,
		TotalDuration: totalDuration,
		InProgress:    inProgress,
		DeveloperName: developerName,
		Status:        statusName,
		UpdateTime:    updateTime,
	}

	if inProgress {
		taskInfo.StartTime = getTaskStartTime(taskID)
		logDebug("Task %s (%s) is in progress, start time: %v", taskID, task.Name, taskInfo.StartTime)
	}

	logInfo("Retrieved task info: %s (%s) - Duration: %v, InProgress: %t, Developer: %s, Status: %s",
		taskID, task.Name, totalDuration, inProgress, developerName, statusName)
	return &taskInfo, nil
}

func checkTaskBelongsToSprint(taskID, sprintID uuid.UUID) bool {
	logDebug("Checking if task %s belongs to sprint %s", taskID, sprintID)

	var count int64
	db.Model(&model.Task{}).Where("\"Id\" = ? AND \"SprintId\" = ?", taskID.String(), sprintID.String()).Count(&count)
	belongs := count > 0
	logDebug("Task %s belongs to sprint %s: %t", taskID, sprintID, belongs)
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
		panic(err)
	}
	defer conn.Close()
	logInfo("Successfully connected to RabbitMQ")

	ch, err := conn.Channel()
	if err != nil {
		logError("Error opening RabbitMQ channel: %v", err)
		log.Fatalf("Error opening channel: %v", err)
	}
	defer ch.Close()
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

	msgs, err := ch.Consume(
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

	for d := range msgs {
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
