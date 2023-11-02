using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class OrderUpdateDTO : VersionDTO
    {
        [Required]
        public ICollection<long> AddMenuItemIds { get; set; }

        [Required]
        public ICollection<long> RemoveMenuItemIds { get; set; }
    }
}
