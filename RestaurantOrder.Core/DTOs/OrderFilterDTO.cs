using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Core.DTOs
{
    public class OrderFilterDTO
    {
        public OrderStatus? Status { get; set; }
        public long? CustomerId { get; set; }
    }
}
