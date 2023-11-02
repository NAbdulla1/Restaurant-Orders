using RestaurantOrder.Core.Validations;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    [NotEmptyMenuItems]
    public class OrderUpdateDTO : VersionDTO
    {
        [Required]
        [ExclusiveMenuItems]
        public ICollection<long> AddMenuItemIds { get; set; }

        [Required]
        [ExclusiveMenuItems]
        public ICollection<long> RemoveMenuItemIds { get; set; }
    }
}
