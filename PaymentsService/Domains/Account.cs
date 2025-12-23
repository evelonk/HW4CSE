namespace PaymentsService.Domain;

public class Account
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public decimal Balance { get; set; }

    public DateTime CreatedAt { get; set; }
}
