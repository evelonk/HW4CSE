namespace OrdersService.Application.Dtos;

public class CreateOrderRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
}
