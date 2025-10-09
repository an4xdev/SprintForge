<?php

namespace App\Services;

use PhpAmqpLib\Connection\AMQPStreamConnection;
use PhpAmqpLib\Message\AMQPMessage;
use Exception;
use Illuminate\Support\Facades\Log;

class AuditService
{
    private $connection;
    private $channel;
    private $serviceName = 'LaravelService';

    public function __construct()
    {
        try {
            $this->connection = new AMQPStreamConnection(
                config('rabbitmq.host'),
                config('rabbitmq.port'),
                config('rabbitmq.user'),
                config('rabbitmq.password')
            );

            $this->channel = $this->connection->channel();

            $this->channel->exchange_declare('audit_logs', 'topic', false, true, false);
        } catch (Exception $e) {
            Log::error('Failed to initialize RabbitMQ connection: ' . $e->getMessage());
        }
    }

    public function logAction(string $action, string $entity, string $description): void
    {
        try {
            if (!$this->channel) {
                Log::error('RabbitMQ channel not available');
                return;
            }

            $auditMessage = [
                'timestamp' => now()->toISOString(),
                'service' => $this->serviceName,
                'action' => $action,
                'entity' => $entity,
                'description' => $description
            ];

            $message = new AMQPMessage(
                json_encode($auditMessage),
                ['content_type' => 'application/json']
            );

            $this->channel->basic_publish($message, 'audit_logs', 'audit.laravelservice');

            Log::debug("Audit log sent: {$action} - {$entity} - {$description}");
        } catch (Exception $e) {
            Log::error('Error publishing audit log: ' . $e->getMessage());
        }
    }

    public function __destruct()
    {
        try {
            if ($this->channel) {
                $this->channel->close();
            }
            if ($this->connection) {
                $this->connection->close();
            }
        } catch (Exception $e) {
            Log::error('Error closing RabbitMQ connection: ' . $e->getMessage());
        }
    }
}
