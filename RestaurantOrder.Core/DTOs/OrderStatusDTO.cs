using RestaurantOrder.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class OrderStatusDTO
    {
        [Required]
        public OrderStatus Status {  get; set; }
    }
}
