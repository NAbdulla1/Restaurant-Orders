using RestaurantOrder.Data.Models;

namespace Restaurant_Orders.Models.DTOs
{
    public class OrderFilterDTO
    {
        public OrderStatus? Status { get; set; }
        public long? CustomerId { get; set; }
    }
}
