using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_Orders.Data.Entities
{
    [Table("users")]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("first_name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [Column("last_name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [Column("email")]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [Column("password")]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [Column("user_type")]
        public UserType UserType { get; set; }
    }
}
