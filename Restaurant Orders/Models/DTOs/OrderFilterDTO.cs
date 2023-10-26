using Restaurant_Orders.Data.Entities;

namespace Restaurant_Orders.Models.DTOs
{
    public class OrderFilterDTO
    {
        public OrderStatus? Status { get; set; }
        public long? CustomerId { get; set; }
    }
}
