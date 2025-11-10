package main

import (
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
    "gorm.io/gorm/schema"
)

var db *gorm.DB

type ApiResponse struct {
    Message string `json:"message"`
    Data    any    `json:"data,omitempty"`
}

type TaskHistory struct {
    ID         uuid.UUID `json:"id" gorm:"column:Id;primaryKey;type:uuid"`
    TaskId     uuid.UUID `json:"task_id" gorm:"column:TaskId;type:uuid"`
    ChangeDate time.Time `json:"change_date" gorm:"column:ChangeDate;type:timestamptz"`
    OldStatus  *string   `json:"old_status" gorm:"column:OldStatus;type:text"`
    NewStatus  string    `json:"new_status" gorm:"column:NewStatus;type:text"`
}

func (TaskHistory) TableName() string {
    return "TaskHistories"
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
                "status": "unhealthy",
                "database": map[string]string{
                    "status": func() string {
                        if dbOK {
                            return "connected"
                        }
                        return "disconnected"
                    }(),
                },
            },
        })
        return
    }

    c.JSON(200, ApiResponse{
        Message: "Service healthy",
        Data: gin.H{
            "status":   "healthy",
            "database": map[string]string{"status": "connected"},
        },
    })
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
        NamingStrategy: schema.NamingStrategy{
            SingularTable: true,
        },
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

func getHistories(c *gin.Context) {
    taskId := c.Query("taskId")
    taskIdIsPresent := strings.TrimSpace(taskId) != ""

    var histories []TaskHistory
    var result *gorm.DB

    if taskIdIsPresent {
        result = db.Where("\"TaskId\" = ?", taskId).Find(&histories)
    } else {
        result = db.Find(&histories)
    }

    if result.Error != nil {
        c.JSON(500, ApiResponse{
            Message: "Failed to retrieve task histories",
            Data:    result.Error.Error(),
        })
        return
    }
    c.JSON(200, ApiResponse{
        Message: "Task histories retrieved successfully",
        Data:    histories,
    })
}

func main() {
    initDatabase()
    r := gin.Default()

    r.GET("/health", healthCheck)
    api := r.Group("/api")
    {
        api.GET("/taskHistories", getHistories)
    }

    port := getEnv("PORT", "8080")
    log.Printf("Reports service starting on port %s", port)
    err := r.Run(":" + port)
    if err != nil {
        log.Fatal("Failed to start server:", err)
        return
    }
}
