using Microsoft.EntityFrameworkCore;
using PaymentsService.Application.Events;
using PaymentsService.Infrastructure;
using PaymentsService.Infrastructure.Inbox;
using PaymentsService.Infrastructure.Messaging;
using PaymentsService.Infrastructure.Outbox;
using System.Text.Json;

namespace PaymentsService.BackgroundServices;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderCreatedConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumer = new RabbitMqConsumer();

                await consumer.StartAsync(async message =>
                {
                    var orderCreated =
                        JsonSerializer.Deserialize<OrderCreatedEvent>(message);

                    if (orderCreated == null)
                        return;

                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider
                        .GetRequiredService<PaymentsDbContext>();
                    var inbox = await db.InboxMessages
                        .SingleOrDefaultAsync(m => m.Id == orderCreated.OrderId);

                    if (inbox != null && inbox.ProcessedAt != null)
                        return;

                    if (inbox == null)
                    {
                        inbox = new InboxMessage
                        {
                            Id = orderCreated.OrderId,
                            Type = nameof(OrderCreatedEvent),
                            ReceivedAt = DateTime.UtcNow
                        };

                        db.InboxMessages.Add(inbox);
                    }

                    var account = await db.Accounts
                        .SingleOrDefaultAsync(a => a.UserId == orderCreated.UserId);
                    if (account == null || account.Balance < orderCreated.Amount)
                    {
                        var failedEvent = new PaymentFailedEvent
                        {
                            OrderId = orderCreated.OrderId,
                            Reason = account == null ? "Account not found" : "Not enough money"
                        };

                        db.OutboxMessages.Add(new OutboxMessage
                        {
                            Id = Guid.NewGuid(),
                            Type = nameof(PaymentFailedEvent),
                            Payload = JsonSerializer.Serialize(failedEvent),
                            OccurredAt = DateTime.UtcNow
                        });

                        inbox.ProcessedAt = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        return;
                    }
                    account.Balance -= orderCreated.Amount;
                    var completedEvent = new PaymentCompletedEvent
                    {
                        OrderId = orderCreated.OrderId
                    };

                    db.OutboxMessages.Add(new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Type = nameof(PaymentCompletedEvent),
                        Payload = JsonSerializer.Serialize(completedEvent),
                        OccurredAt = DateTime.UtcNow
                    });

                    inbox.ProcessedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                });
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"RabbitMQ not ready, retrying in 5 seconds... {ex.Message}");

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
