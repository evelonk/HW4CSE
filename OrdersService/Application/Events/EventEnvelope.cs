namespace OrdersService.Application.Events;

public class EventEnvelope
{
    public string Type { get; set; } = null!;
    public string Payload { get; set; } = null!;
}
