import pika
import json
import os
from typing import Optional


class RabbitMQConfig:

    def __init__(self):
        self.host = os.getenv("RABBITMQ_HOST")
        self.port = int(os.getenv("RABBITMQ_PORT"))
        self.user = os.getenv("RABBITMQ_USER")
        self.password = os.getenv("RABBITMQ_PASS")
        self.connection: Optional[pika.BlockingConnection] = None
        self.channel: Optional[pika.channel.Channel] = None
    
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
            self.channel.queue_declare(queue='task_queue', durable=True)
            return True
        except Exception as e:
            print(f"RabbitMQ connection failed: {e}")
            return False
    
    def is_connected(self):
        try:
            if self.connection and not self.connection.is_closed:
                return True
            return self.connect()
        except Exception:
            return False
    
    def publish_message(self, message: dict):
        if not self.is_connected():
            raise Exception("RabbitMQ not connected")
        
        if self.channel is None:
            raise Exception("RabbitMQ channel not available")
        
        self.channel.basic_publish(
            exchange='',
            routing_key='task_queue',
            body=json.dumps(message),
            properties=pika.BasicProperties(delivery_mode=2)
        )
