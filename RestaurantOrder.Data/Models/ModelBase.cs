using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrder.Data.Models
{
    public abstract class ModelBase
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
    }
}
