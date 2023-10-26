using Restaurant_Orders.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Orders.Models.DTOs
{
    public class OrderStatusDTO : VersionDTO
    {
        [Required]
        public OrderStatus Status {  get; set; }
    }
}
