using RestaurantOrder.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class MenuItemDTO : MenuItemUpdateDTO
    {
        [Required] public long Id { get; set; }

        public MenuItem ToMenuItem()
        {
            return new MenuItem
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Price = Price
            };
        }
    }
}
