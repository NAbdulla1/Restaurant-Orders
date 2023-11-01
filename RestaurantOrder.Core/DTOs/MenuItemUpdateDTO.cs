using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class MenuItemUpdateDTO
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.PositiveInfinity)]
        public decimal Price { get; set; }
    }
}
