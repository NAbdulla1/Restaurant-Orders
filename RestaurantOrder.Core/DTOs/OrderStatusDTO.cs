using RestaurantOrder.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class OrderStatusDTO : VersionDTO
    {
        [Required]
        public OrderStatus Status {  get; set; }
    }
}
