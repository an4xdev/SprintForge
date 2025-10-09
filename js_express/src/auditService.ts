import amqp, { Connection, Channel, ChannelModel } from 'amqplib';

export class AuditService {
    private connection?: ChannelModel;
    private channel?: Channel;
    private serviceName = 'JSExpressService';

    async initialize(): Promise<void> {
        try {
            const rabbitmqHost = process.env.RABBITMQ_HOST || 'rabbitmq';
            const rabbitmqPort = process.env.RABBITMQ_PORT || '5672';
            const rabbitmqUser = process.env.RABBITMQ_USER || 'user';
            const rabbitmqPass = process.env.RABBITMQ_PASS || 'password';

            const connectionString = `amqp://${rabbitmqUser}:${rabbitmqPass}@${rabbitmqHost}:${rabbitmqPort}`;

            this.connection = await amqp.connect(connectionString);
            this.channel = await this.connection.createChannel();

            await this.channel.assertExchange('audit_logs', 'topic', { durable: true });

            console.log('RabbitMQ audit service initialized');
        } catch (error) {
            console.error('Failed to initialize RabbitMQ connection:', error);
        }
    }

    async logAction(action: string, entity: string, description: string): Promise<void> {
        try {
            if (!this.channel) {
                console.error('RabbitMQ channel not available');
                return;
            }

            const auditMessage = {
                timestamp: new Date().toISOString(),
                service: this.serviceName,
                action,
                entity,
                description
            };

            const messageBuffer = Buffer.from(JSON.stringify(auditMessage));

            this.channel.publish('audit_logs', 'audit.jsexpressservice', messageBuffer);

            console.log(`Audit log sent: ${action} - ${entity} - ${description}`);
        } catch (error) {
            console.error('Error publishing audit log:', error);
        }
    }

    async close(): Promise<void> {
        try {
            if (this.channel) {
                await this.channel.close();
            }
            if (this.connection) {
                await this.connection.close();
            }
        } catch (error) {
            console.error('Error closing RabbitMQ connection:', error);
        }
    }
}

export const auditService = new AuditService();