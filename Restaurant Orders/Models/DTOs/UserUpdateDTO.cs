using Restaurant_Orders.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Orders.Models.DTOs
{
    public class UserUpdateDTO
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match.")]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords don't match.")]
        public string NewPassword { get; set; }
    }
}
