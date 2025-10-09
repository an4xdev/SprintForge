using RabbitMQ.Client;

namespace AuthService.Services;

public class RabbitMqConnectionManager : IAsyncDisposable
{
    public IConnection Connection { get; }

    public RabbitMqConnectionManager(string connectionString)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = true
        };
        Connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    public ValueTask DisposeAsync()
    {
        Connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
