using RestaurantOrder.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Orders.Models.DTOs
{
    public class OrderStatusDTO : VersionDTO
    {
        [Required]
        public OrderStatus Status {  get; set; }
    }
}
