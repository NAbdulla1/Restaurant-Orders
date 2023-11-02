using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class NewOrderDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least 1 menu item id is required in the list")]
        public ICollection<long> menuItemIds { get; set; }
    }
}
