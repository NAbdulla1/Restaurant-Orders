using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantOrder.Core.Extensions
{
    public static class MenuItemExtensions
    {
        public static MenuItemDTO toMenuItemDTO(this MenuItem menuItem)
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
