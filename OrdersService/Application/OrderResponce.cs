using OrdersService.Domain;

namespace OrdersService.Application.Dtos;

public class OrderResponse
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
}
