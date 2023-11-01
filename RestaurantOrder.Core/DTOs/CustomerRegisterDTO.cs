using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class CustomerRegisterDTO
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email {  get; set; }

        [Required]
        [StringLength(255)]
        [Compare(nameof(ConfirmPassword), ErrorMessage = "Passwords don't match.")]
        public string Password { get; set; }

        [Required]
        [StringLength(255)]
        [Compare(nameof(Password), ErrorMessage = "Passwords don't match.")]
        public string ConfirmPassword { get; set; }
    }
}
