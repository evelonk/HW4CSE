namespace PaymentsService.Application.Events
{
    public class PaymentCompletedEvent
    {
        public Guid OrderId { get; set; }
    }
}
