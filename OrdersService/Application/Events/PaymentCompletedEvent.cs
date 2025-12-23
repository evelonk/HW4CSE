namespace OrdersService.Application.Events;

public class PaymentCompletedEvent
{
    public Guid OrderId { get; set; }
}
