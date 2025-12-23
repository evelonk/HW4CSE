namespace PaymentsService.Infrastructure.Inbox;

public class InboxMessage
{
    public Guid Id { get; set; }          

    public string Type { get; set; } = null!;

    public DateTime ReceivedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
