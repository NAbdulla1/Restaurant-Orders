using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Core.Extensions
{
    public static class OrderExtensions
    {
        public static OrderDTO ToOrderDTO(this Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                Total = order.Total,
                Version = order.Version,
                OrderItems = order.OrderItems?.Select(orderItem => orderItem.ToOrderItemDTO())
            };
        }
    }
}
