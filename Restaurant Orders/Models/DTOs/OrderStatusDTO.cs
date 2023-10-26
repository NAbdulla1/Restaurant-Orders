using Restaurant_Orders.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Orders.Models.DTOs
{
    public class OrderStatusDTO
    {
        [Required]
        public OrderStatus Status {  get; set; }

        [Required]
        public Guid? Version { get; set; }
    }
}
