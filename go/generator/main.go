package main

import (
	"log"

	"gorm.io/driver/postgres"
	"gorm.io/gen"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"
)

func main() {
	dsn := "host=localhost user=postgres password=P@ssword123! dbname=project port=5432 sslmode=disable TimeZone=Europe/Warsaw"

	gormdb, err := gorm.Open(postgres.Open(dsn), &gorm.Config{
		Logger: logger.Default.LogMode(logger.Info),
	})
	if err != nil {
		log.Fatal("Failed to connect to database:", err)
	}

	g := gen.NewGenerator(gen.Config{
		OutPath:           "./models",
		OutFile:           "generated.go",
		ModelPkgPath:      "./model",
		WithUnitTest:      false,
		FieldNullable:     false,
		FieldCoverable:    false,
		FieldSignable:     false,
		FieldWithIndexTag: false,
		FieldWithTypeTag:  true,
		Mode:              gen.WithoutContext | gen.WithDefaultQuery | gen.WithQueryInterface,
	})

	g.UseDB(gormdb)

	sqlDB, err := gormdb.DB()
	if err != nil {
		log.Fatal("Failed to get underlying sql.DB:", err)
	}

	if err := sqlDB.Ping(); err != nil {
		log.Fatal("Failed to ping database:", err)
	}

	log.Println("Connected to database successfully!")

	allTables := g.GenerateAllTable()
	if len(allTables) == 0 {
		log.Println("No tables found in database")
		return
	}

	log.Printf("Found %d tables to generate", len(allTables))

	g.ApplyBasic(
		g.GenerateModel("Companies"),
		g.GenerateModel("Projects"),
		g.GenerateModel("Sprints"),
		g.GenerateModel("TaskHistories"),
		g.GenerateModel("TaskStatuses"),
		g.GenerateModel("TaskTypes"),
		g.GenerateModel("Tasks"),
		g.GenerateModel("Teams"),
		g.GenerateModel("Users"),
	)

	g.Execute()

	log.Println("Models generated successfully!")
	log.Println("Generated files:")
	log.Println("  - ./query/generated.go")
	log.Println("  - ./model/ (model files)")
}
