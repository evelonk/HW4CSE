using OrdersService.Domain;

namespace OrdersService.Application.Dtos;

public class OrderListItemResponse
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
