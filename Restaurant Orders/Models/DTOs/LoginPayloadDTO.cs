using System.ComponentModel.DataAnnotations;

namespace Restaurant_Orders.Models.DTOs
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
