namespace PaymentsService.Application.Dtos;

public class AccountResponse
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
}
