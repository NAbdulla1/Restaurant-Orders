using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class LoginPayloadDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
