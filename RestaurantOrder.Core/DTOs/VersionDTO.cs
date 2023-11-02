using RestaurantOrder.Core.Validations;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class VersionDTO
    {
        [Required]
        [RequiredGuid]
        public Guid Version { get; set; }
    }
}
