using System.ComponentModel.DataAnnotations;

namespace Restaurant_Orders.Models.DTOs
{
    public class NewOrderDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least 1 menu item id is required in the list")]
        public ICollection<long> menuItemIds { get; set; }
    }
}
