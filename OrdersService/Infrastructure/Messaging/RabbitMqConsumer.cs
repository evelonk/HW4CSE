using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace OrdersService.Infrastructure.Messaging;

public class RabbitMqConsumer
{
    private readonly ConnectionFactory _factory;

    public RabbitMqConsumer()
    {
        _factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672
        };
    }

    public async Task StartAsync(
        string exchange,
        string queue,
        string routingKey,
        Func<string, Task> handleMessage)
    {
        while (true)
        {
            try
            {
                var connection = await _factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                await channel.ExchangeDeclareAsync(
                    exchange: exchange,
                    type: ExchangeType.Fanout,
                    durable: true);

                await channel.QueueDeclareAsync(
                    queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                await channel.QueueBindAsync(
                    queue: queue,
                    exchange: exchange,
                    routingKey: routingKey);

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (_, ea) =>
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                    try
                    {
                        await handleMessage(message);
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Message handling error: {ex.Message}");

                        await channel.BasicNackAsync(
                            ea.DeliveryTag,
                            false,
                            requeue: true);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: queue,
                    autoAck: false,
                    consumer: consumer);

                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"RabbitMQ not ready, retrying in 5 seconds... {ex.Message}");

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
