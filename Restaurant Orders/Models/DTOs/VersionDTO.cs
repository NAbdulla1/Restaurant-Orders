using System.ComponentModel.DataAnnotations;

namespace Restaurant_Orders.Models.DTOs
{
    public class VersionDTO
    {
        [Required]
        public Guid? Version { get; set; }
    }
}
