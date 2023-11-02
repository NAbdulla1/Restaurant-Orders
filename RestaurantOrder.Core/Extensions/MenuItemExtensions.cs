using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Core.Extensions
{
    public static class MenuItemExtensions
    {
        public static MenuItemDTO ToMenuItemDTO(this MenuItem menuItem)
        {
            return new MenuItemDTO
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
            };
        }
    }
}
