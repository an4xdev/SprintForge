import pika
import json
import os
from typing import Optional
from datetime import datetime


class AuditService:
    def __init__(self):
        self.host = os.getenv("RABBITMQ_HOST", "rabbitmq")
        self.port = int(os.getenv("RABBITMQ_PORT", "5672"))
        self.user = os.getenv("RABBITMQ_USER", "user")
        self.password = os.getenv("RABBITMQ_PASS", "password")
        self.connection: Optional[pika.BlockingConnection] = None
        self.channel: Optional[pika.channel.Channel] = None
        self.service_name = "FastAPIService"
    
    def connect(self):
        try:
            credentials = pika.PlainCredentials(self.user, self.password)
            parameters = pika.ConnectionParameters(
                host=self.host,
                port=self.port,
                credentials=credentials
            )
            
            self.connection = pika.BlockingConnection(parameters)
            self.channel = self.connection.channel()
            
            self.channel.exchange_declare(exchange='audit_logs', exchange_type='topic', durable=True)
            
            return True
        except Exception as e:
            print(f"RabbitMQ audit connection failed: {e}")
            return False
    
    def is_connected(self):
        try:
            if self.connection and not self.connection.is_closed:
                return True
            return self.connect()
        except Exception:
            return False
    
    def log_action(self, action: str, entity: str, description: str):
        try:
            if not self.is_connected():
                print("RabbitMQ audit not connected")
                return
            
            if self.channel is None:
                print("RabbitMQ audit channel not available")
                return
            
            audit_message = {
                "timestamp": datetime.utcnow().isoformat(),
                "service": self.service_name,
                "action": action,
                "entity": entity,
                "description": description
            }
            
            self.channel.basic_publish(
                exchange='audit_logs',
                routing_key='audit.fastapiservice',
                body=json.dumps(audit_message),
                properties=pika.BasicProperties(delivery_mode=2)
            )
            
            print(f"Audit log sent: {action} - {entity} - {description}")
        except Exception as e:
            print(f"Error publishing audit log: {e}")
    
    def close(self):
        try:
            if self.channel:
                self.channel.close()
            if self.connection:
                self.connection.close()
        except Exception as e:
            print(f"Error closing RabbitMQ audit connection: {e}")


audit_service = AuditService()