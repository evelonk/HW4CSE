using RabbitMQ.Client;
using System.Text;

namespace OrdersService.Infrastructure.Messaging;

public class RabbitMqPublisher
{
    private IConnection? _connection;

    private async Task<IConnection> GetConnectionAsync()
    {
        if (_connection != null && _connection.IsOpen)
            return _connection;

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672
        };

        _connection = await factory.CreateConnectionAsync();
        return _connection;
    }

    public async Task PublishAsync(string messageType, string payload)
    {
        var connection = await GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "orders",
            type: ExchangeType.Fanout,
            durable: true);

        var body = Encoding.UTF8.GetBytes(payload);

        await channel.BasicPublishAsync(
            exchange: "orders",
            routingKey: "",
            body: body);
    }
}
