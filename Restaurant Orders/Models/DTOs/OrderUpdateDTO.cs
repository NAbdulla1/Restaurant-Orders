using System.ComponentModel.DataAnnotations;

namespace Restaurant_Orders.Models.DTOs
{
    public class OrderUpdateDTO : VersionDTO
    {
        [Required]
        public ICollection<long> AddMenuItemIds { get; set; }

        [Required]
        public ICollection<long> RemoveMenuItemIds { get; set; }
    }
}
