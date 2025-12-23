using Microsoft.EntityFrameworkCore;
using PaymentsService.Application.Events;
using PaymentsService.Infrastructure;
using PaymentsService.Infrastructure.Messaging;
using System.Text.Json;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxProcessor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<PaymentsDbContext>();

            var publisher = scope.ServiceProvider
                .GetRequiredService<RabbitMqPublisher>();

            var messages = await db.OutboxMessages
                .Where(x => x.ProcessedAt == null)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                var envelope = new EventEnvelope
                {
                    Type = message.Type,
                    Payload = message.Payload
                };

                await publisher.PublishAsync(
                    JsonSerializer.Serialize(envelope),
                    stoppingToken
                );

                message.ProcessedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
