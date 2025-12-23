namespace OrdersService.Application.Events;

public class PaymentFailedEvent
{
    public Guid OrderId { get; set; }
}
