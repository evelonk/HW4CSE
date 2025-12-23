using Microsoft.EntityFrameworkCore;
using OrdersService.Application.Events;
using OrdersService.Domain;
using OrdersService.Infrastructure;
using OrdersService.Infrastructure.Messaging;
using System.Text.Json;

namespace OrdersService.BackgroundServices;

public class PaymentCompletedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqConsumer _consumer = new();

    public PaymentCompletedConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartAsync(
            exchange: "payments",
            queue: "orders.payment.completed",
            routingKey: "",
            HandleAsync);
    }

    private async Task HandleAsync(string message)
    {
        var envelope = JsonSerializer.Deserialize<EventEnvelope>(message);
        if (envelope == null)
            return;

        if (envelope.Type != nameof(PaymentCompletedEvent))
            return;

        var evt = JsonSerializer.Deserialize<PaymentCompletedEvent>(envelope.Payload);
        if (evt == null)
            return;

        if (evt == null) return;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == evt.OrderId);
        if (order == null || order.Status != OrderStatus.Pending)
            return;

        order.Status = OrderStatus.Paid;
        await db.SaveChangesAsync();
    }
}
