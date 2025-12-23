namespace PaymentsService.Application.Events;

public class PaymentResultEvent
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = null!;
    public string? Reason { get; set; }
}