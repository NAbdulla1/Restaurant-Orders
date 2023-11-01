using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class UserUpdateDTO
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(255)]
        [Compare(nameof(ConfirmPasword), ErrorMessage = "Passwords don't match.")]
        public string? NewPassword { get; set; }

        [StringLength(255)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match.")]
        public string? ConfirmPasword { get; set; }
    }
}
