namespace PaymentsService.Application.Events
{
    public class PaymentFailedEvent
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; } = null!;
    }   
}
