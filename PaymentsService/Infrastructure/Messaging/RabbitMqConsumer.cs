using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace PaymentsService.Infrastructure.Messaging;

public class RabbitMqConsumer
{
    public async Task StartAsync(Func<string, Task> handleMessage)
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "orders",
            type: ExchangeType.Fanout,
            durable: true);

        var queue = await channel.QueueDeclareAsync();
        await channel.QueueBindAsync(queue.QueueName, "orders", "");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            await handleMessage(body);
        };

        await channel.BasicConsumeAsync(queue.QueueName, autoAck: true, consumer);
    }
}
