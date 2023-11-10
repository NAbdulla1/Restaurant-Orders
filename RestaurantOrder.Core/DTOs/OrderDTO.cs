using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Core.DTOs
{
    public class OrderDTO
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid Version { get; set; }

        public IEnumerable<OrderItemDTO> OrderItems { get; set; }

        public Order ToOrder()
        {
            Enum.TryParse<OrderStatus>(Status, out var statusValue);
            return new Order
            {
                Id = Id,
                CustomerId = CustomerId,
                Total = Total,
                CreatedAt = CreatedAt,
                Status = statusValue,
                OrderItems = OrderItems.Select(orderItem => orderItem.ToOrderItem()).ToList()
            };
        }
    }
}
