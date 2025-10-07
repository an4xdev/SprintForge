package main

import (
	"database/sql"
	"fmt"
	"log"
	"os"
	"strings"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
	_ "github.com/jackc/pgx/v5/stdlib"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"
)

var db *gorm.DB

type ApiResponse struct {
	Message string      `json:"message"`
	Data    interface{} `json:"data,omitempty"`
}

func parseUUIDArray(arrayStr string) ([]uuid.UUID, error) {
	if arrayStr == "" || arrayStr == "{}" {
		return []uuid.UUID{}, nil
	}

	arrayStr = strings.Trim(arrayStr, "{}")
	if arrayStr == "" {
		return []uuid.UUID{}, nil
	}

	parts := strings.Split(arrayStr, ",")
	uuids := make([]uuid.UUID, 0, len(parts))

	for _, part := range parts {
		part = strings.TrimSpace(part)
		if part != "" {
			u, err := uuid.Parse(part)
			if err != nil {
				return nil, fmt.Errorf("failed to parse UUID '%s': %w", part, err)
			}
			uuids = append(uuids, u)
		}
	}

	return uuids, nil
}

func parseTextArray(arrayStr string) []string {
	if arrayStr == "" || arrayStr == "{}" {
		return []string{}
	}

	arrayStr = strings.Trim(arrayStr, "{}")
	if arrayStr == "" {
		return []string{}
	}

	parts := strings.Split(arrayStr, ",")
	result := make([]string, 0, len(parts))

	for _, part := range parts {
		part = strings.TrimSpace(part)
		if len(part) >= 2 && part[0] == '"' && part[len(part)-1] == '"' {
			part = part[1 : len(part)-1]
		}
		if part != "" {
			result = append(result, part)
		}
	}

	return result
}

// SprintReport model for GetSprintsReport procedure
type SprintReport struct {
	SprintID           uuid.UUID `json:"sprintId"`
	SprintName         string    `json:"sprintName"`
	TaskCount          int       `json:"taskCount"`
	TaskCountCompleted int       `json:"taskCountCompleted"`
	TotalTaskTime      string    `json:"totalTaskTime"`
	CompletedRatio     *float64  `json:"completedRatio"`
}

// TeamReport model for GetTeamReport procedure
type TeamReport struct {
	DeveloperCount     int         `json:"developerCount"`
	DeveloperIds       []uuid.UUID `json:"developerIds"`
	SprintsNames       []string    `json:"sprintsNames"`
	TaskCount          int         `json:"taskCount"`
	TaskCountCompleted int         `json:"taskCountCompleted"`
	TotalTaskTime      string      `json:"totalTaskTime"`
}

// ProjectReport model for GetProjectsReport procedure
type ProjectReport struct {
	ProjectID          uuid.UUID  `json:"projectId"`
	ProjectName        string     `json:"projectName"`
	CompanyName        string     `json:"companyName"`
	SprintCount        int        `json:"sprintCount"`
	TaskCount          int        `json:"taskCount"`
	TaskCountCompleted int        `json:"taskCountCompleted"`
	TotalTaskTime      string     `json:"totalTaskTime"`
	ProjectStartDate   *time.Time `json:"projectStartDate"`
	ProjectEndDate     *time.Time `json:"projectEndDate"`
	CompletedRatio     *float64   `json:"completedRatio"`
}

func checkDatabaseConnection() bool {
	sqlDB, err := db.DB()
	if err != nil {
		return false
	}
	return sqlDB.Ping() == nil
}

func healthCheck(c *gin.Context) {
	dbOK := checkDatabaseConnection()

	if !dbOK {
		c.JSON(503, ApiResponse{
			Message: "Service unavailable",
			Data: gin.H{
				"status":   "unhealthy",
				"database": "disconnected",
			},
		})
		return
	}

	c.JSON(200, ApiResponse{
		Message: "Service healthy",
		Data: gin.H{
			"status":   "healthy",
			"database": "connected",
		},
	})
}

func main() {
	initDatabase()

	r := gin.Default()

	r.GET("/health", healthCheck)
	api := r.Group("/api")
	{
		api.GET("/reports/sprints", getSprintsReport)
		api.GET("/reports/teams", getTeamsReport)
		api.GET("/reports/projects", getProjectsReport)
	}

	port := getEnv("PORT", "8080")
	log.Printf("Reports service starting on port %s", port)
	err := r.Run(":" + port)
	if err != nil {
		log.Fatal("Failed to start server:", err)
		return
	}
}

func initDatabase() {
	host := getEnv("DB_HOST", "db")
	port := getEnv("DB_PORT", "5432")
	user := getEnv("DB_USER", "postgres")
	password := getEnv("DB_PASSWORD", "password")
	dbname := getEnv("DB_DATABASE", "project")

	dsn := fmt.Sprintf("host=%s user=%s password=%s dbname=%s port=%s sslmode=disable TimeZone=UTC",
		host, user, password, dbname, port)

	var err error

	db, err = gorm.Open(postgres.New(postgres.Config{
		DriverName: "pgx",
		DSN:        dsn,
	}), &gorm.Config{
		Logger: logger.Default.LogMode(logger.Info),
	})

	if err != nil {
		log.Fatal("Failed to connect to database:", err)
	}

	log.Println("Connected to database successfully")
}

func getEnv(key, defaultValue string) string {
	if value := os.Getenv(key); value != "" {
		return value
	}
	return defaultValue
}

// GET /api/reports/sprints?managerId={uuid}&startDate={date}&endDate={date}
func getSprintsReport(c *gin.Context) {
	managerID := c.Query("managerId")
	startDate := c.Query("startDate")
	endDate := c.Query("endDate")

	var args []interface{}
	var managerUUID *uuid.UUID
	var startDateParsed *time.Time
	var endDateParsed *time.Time

	if managerID != "" {
		if parsed, err := uuid.Parse(managerID); err != nil {
			c.JSON(400, ApiResponse{Message: "Invalid manager ID format"})
			return
		} else {
			managerUUID = &parsed
		}
	}

	if startDate != "" {
		if parsed, err := time.Parse("2006-01-02", startDate); err != nil {
			c.JSON(400, ApiResponse{Message: "Invalid start date format. Use YYYY-MM-DD"})
			return
		} else {
			startDateParsed = &parsed
		}
	}

	if endDate != "" {
		if parsed, err := time.Parse("2006-01-02", endDate); err != nil {
			c.JSON(400, ApiResponse{Message: "Invalid end date format. Use YYYY-MM-DD"})
			return
		} else {
			endDateParsed = &parsed
		}
	}

	if managerUUID != nil {
		args = append(args, *managerUUID)
	} else {
		args = append(args, nil)
	}

	if startDateParsed != nil {
		args = append(args, *startDateParsed)
	} else {
		args = append(args, nil)
	}

	if endDateParsed != nil {
		args = append(args, *endDateParsed)
	} else {
		args = append(args, nil)
	}

	query := `SELECT * FROM GetSprintsReport($1, $2, $3)`

	rows, err := db.Raw(query, args...).Rows()
	if err != nil {
		c.JSON(500, ApiResponse{Message: "Database error: " + err.Error()})
		return
	}
	defer func(rows *sql.Rows) {
		err := rows.Close()
		if err != nil {
			log.Println("Error closing rows:", err)
		}
	}(rows)

	var reports []SprintReport
	for rows.Next() {
		var report SprintReport
		var totalTaskTime sql.NullString
		var completedRatio sql.NullFloat64

		err := rows.Scan(
			&report.SprintID,
			&report.SprintName,
			&report.TaskCount,
			&report.TaskCountCompleted,
			&totalTaskTime,
			&completedRatio,
		)
		if err != nil {
			c.JSON(500, ApiResponse{Message: "Scan error: " + err.Error()})
			return
		}

		if totalTaskTime.Valid {
			report.TotalTaskTime = totalTaskTime.String
		} else {
			report.TotalTaskTime = "00:00:00"
		}

		if completedRatio.Valid {
			report.CompletedRatio = &completedRatio.Float64
		}

		reports = append(reports, report)
	}

	c.JSON(200, ApiResponse{
		Message: "Sprints report retrieved",
		Data:    reports,
	})
}

// GET /api/reports/teams?managerId={uuid}&startDate={date}&endDate={date}
func getTeamsReport(c *gin.Context) {
	managerID := c.Query("managerId")
	startDate := c.Query("startDate")
	endDate := c.Query("endDate")

	var args []interface{}
	var managerUUID *uuid.UUID
	var startDateParsed *time.Time
	var endDateParsed *time.Time

	if managerID != "" {
		if parsed, err := uuid.Parse(managerID); err != nil {
			c.JSON(400, ApiResponse{Message: "Invalid manager ID format"})
			return
		} else {
			managerUUID = &parsed
		}
	}

	if startDate != "" {
		if parsed, err := time.Parse("2006-01-02", startDate); err != nil {
			c.JSON(400, ApiResponse{Message: "Invalid start date format. Use YYYY-MM-DD"})
			return
		} else {
			startDateParsed = &parsed
		}
	}

	if endDate != "" {
		if parsed, err := time.Parse("2006-01-02", endDate); err != nil {
			c.JSON(400, ApiResponse{Message: "Invalid end date format. Use YYYY-MM-DD"})
			return
		} else {
			endDateParsed = &parsed
		}
	}

	if managerUUID != nil {
		args = append(args, *managerUUID)
	} else {
		args = append(args, nil)
	}

	if startDateParsed != nil {
		args = append(args, *startDateParsed)
	} else {
		args = append(args, nil)
	}

	if endDateParsed != nil {
		args = append(args, *endDateParsed)
	} else {
		args = append(args, nil)
	}

	query := `SELECT * FROM GetTeamsReport($1, $2, $3)`

	rows, err := db.Raw(query, args...).Rows()
	if err != nil {
		c.JSON(500, ApiResponse{Message: "Database error: " + err.Error()})
		return
	}
	defer func(rows *sql.Rows) {
		err := rows.Close()
		if err != nil {
			log.Println("Error closing rows:", err)
		}
	}(rows)

	var reports []TeamReport
	for rows.Next() {
		var report TeamReport
		var totalTaskTime sql.NullString
		var developerIdsStr sql.NullString
		var sprintsNamesStr sql.NullString

		err := rows.Scan(
			&report.DeveloperCount,
			&developerIdsStr,
			&sprintsNamesStr,
			&report.TaskCount,
			&report.TaskCountCompleted,
			&totalTaskTime,
		)
		if err != nil {
			c.JSON(500, ApiResponse{Message: "Scan error: " + err.Error()})
			return
		}

		if developerIdsStr.Valid {
			uuids, err := parseUUIDArray(developerIdsStr.String)
			if err != nil {
				c.JSON(500, ApiResponse{Message: "Error parsing developer IDs: " + err.Error()})
				return
			}
			report.DeveloperIds = uuids
		} else {
			report.DeveloperIds = []uuid.UUID{}
		}

		if sprintsNamesStr.Valid {
			report.SprintsNames = parseTextArray(sprintsNamesStr.String)
		} else {
			report.SprintsNames = []string{}
		}

		if totalTaskTime.Valid {
			report.TotalTaskTime = totalTaskTime.String
		} else {
			report.TotalTaskTime = "00:00:00"
		}

		reports = append(reports, report)
	}

	c.JSON(200, ApiResponse{
		Message: "Teams report retrieved",
		Data:    reports,
	})
}

// GET /api/reports/teams?managerId={uuid}&startDate={date}&endDate={date}
func getProjectsReport(c *gin.Context) {
	managerID := c.Query("managerId")
	startDate := c.Query("startDate")
	endDate := c.Query("endDate")

	var args []interface{}
	var managerUUID *uuid.UUID
	var startDateParsed *time.Time
	var endDateParsed *time.Time

	if managerID != "" {
		if parsed, err := uuid.Parse(managerID); err != nil {
			c.JSON(400, ApiResponse{Message: "Invalid manager ID format"})
			return
		} else {
			managerUUID = &parsed
		}
	}

	if startDate != "" {
		if parsed, err := time.Parse("2006-01-02", startDate); err != nil {
			c.JSON(400, ApiResponse{Message: "Invalid start date format. Use YYYY-MM-DD"})
			return
		} else {
			startDateParsed = &parsed
		}
	}

	if endDate != "" {
		if parsed, err := time.Parse("2006-01-02", endDate); err != nil {
			c.JSON(400, ApiResponse{Message: "Invalid end date format. Use YYYY-MM-DD"})
			return
		} else {
			endDateParsed = &parsed
		}
	}

	if managerUUID != nil {
		args = append(args, *managerUUID)
	} else {
		args = append(args, nil)
	}

	if startDateParsed != nil {
		args = append(args, *startDateParsed)
	} else {
		args = append(args, nil)
	}

	if endDateParsed != nil {
		args = append(args, *endDateParsed)
	} else {
		args = append(args, nil)
	}

	query := `SELECT * FROM GetProjectsReport($1, $2, $3)`

	rows, err := db.Raw(query, args...).Rows()
	if err != nil {
		c.JSON(500, ApiResponse{Message: "Database error: " + err.Error()})
		return
	}
	defer func(rows *sql.Rows) {
		err := rows.Close()
		if err != nil {
			log.Println("Error closing rows:", err)
		}
	}(rows)

	var reports []ProjectReport
	for rows.Next() {
		var report ProjectReport
		var totalTaskTime sql.NullString
		var completedRatio sql.NullFloat64

		err := rows.Scan(
			&report.ProjectID,
			&report.ProjectName,
			&report.CompanyName,
			&report.SprintCount,
			&report.TaskCount,
			&report.TaskCountCompleted,
			&totalTaskTime,
			&report.ProjectStartDate,
			&report.ProjectEndDate,
			&completedRatio,
		)
		if err != nil {
			c.JSON(500, ApiResponse{Message: "Scan error: " + err.Error()})
			return
		}

		if totalTaskTime.Valid {
			report.TotalTaskTime = totalTaskTime.String
		} else {
			report.TotalTaskTime = "00:00:00"
		}

		if completedRatio.Valid {
			report.CompletedRatio = &completedRatio.Float64
		}

		reports = append(reports, report)
	}

	c.JSON(200, ApiResponse{
		Message: "Projects report retrieved",
		Data:    reports,
	})
}
