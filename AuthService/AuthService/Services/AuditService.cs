using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuthService.Services;

public interface IAuditService
{
    Task SendAuditLogAsync(string action, string entity, string description);
}

public class AuditService(IConnection connection, ILogger<AuditService> logger) : IAuditService
{
    private readonly string _serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "AuthService";

    public async Task SendAuditLogAsync(string action, string entity, string description)
    {
        try
        {
            await using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "audit_logs",
                type: "topic",
                durable: true,
                autoDelete: false,
                arguments: null
            );

            var auditMessage = new
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                service = _serviceName,
                action,
                entity,
                description
            };

            var json = JsonSerializer.Serialize(auditMessage);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: "audit_logs",
                routingKey: "audit.authservice",
                body: body
            );

            logger.LogInformation("Audit log sent to RabbitMQ: {Action} {Entity}", action, entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send audit log to RabbitMQ");
        }

        await Task.CompletedTask;
    }
}