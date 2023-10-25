using System.ComponentModel.DataAnnotations;

namespace Restaurant_Orders.Models.DTOs
{
    public class OrderUpdateDTO
    {
        [Required]
        public ICollection<long> AddMenuItemIds { get; set; }

        [Required]
        public ICollection<long> RemoveMenuItemIds { get; set; }

        [Required]
        public Guid? Version { get; set; }
    }
}
