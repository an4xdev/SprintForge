package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"

	"github.com/gorilla/websocket"
	"github.com/streadway/amqp"
)

var (
	clients   = make(map[*websocket.Conn]bool)
	broadcast = make(chan string)
	upgrader  = websocket.Upgrader{
		CheckOrigin: func(r *http.Request) bool { return true },
	}
)

type RabbitMQConfig struct {
	Host     string
	Port     string
	User     string
	Password string
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
	ws, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("Upgrade error:", err)
		return
	}
	defer ws.Close()

	clients[ws] = true

	for {
		_, msg, err := ws.ReadMessage()
		if err != nil {
			log.Println("ReadMessage error:", err)
			delete(clients, ws)
			break
		}
		log.Printf("Received from client: %s", msg)
	}
}

func handleMessages() {
	for {
		msg := <-broadcast
		for client := range clients {
			err := client.WriteMessage(websocket.TextMessage, []byte(msg))
			if err != nil {
				log.Printf("Error sending message: %v", err)
				client.Close()
				delete(clients, client)
			}
		}
	}
}

func consumeRabbitMQ() {
	var conn *amqp.Connection
	var err error

	config := getRabbitMQConfig()
	connURL := fmt.Sprintf("amqp://%s:%s@%s:%s/",
		config.User, config.Password, config.Host, config.Port)

	conn, err = amqp.Dial(connURL)

	if err != nil {
		log.Fatalf("Failed to connect to RabbitMQ after 5 attempts: %v", err)
		panic(err)
	}
	defer conn.Close()

	ch, err := conn.Channel()
	if err != nil {
		log.Fatalf("Error opening channel: %v", err)
	}
	defer ch.Close()

	q, err := ch.QueueDeclare(
		"task_queue",
		true,
		false,
		false,
		false,
		nil,
	)
	if err != nil {
		log.Fatalf("Error declaring queue: %v", err)
	}

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
		log.Fatalf("Error consuming messages: %v", err)
	}

	for d := range msgs {
		log.Printf("Received message from queue: %s", d.Body)
		broadcast <- string(d.Body)
	}
}

func main() {
	go consumeRabbitMQ()

	http.HandleFunc("/ws", handleConnections)
	http.HandleFunc("/health", healthHandler)

	go handleMessages()

	log.Println("Go server with WebSocket on port :8080")
	if err := http.ListenAndServe("localhost:8080", nil); err != nil {
		log.Fatal("ListenAndServe: ", err)
	}
}
