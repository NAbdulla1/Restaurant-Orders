using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Core.Extensions
{
    public static class OrderItemExtensions
    {
        public static OrderItemDTO ToOrderItemDTO(this OrderItem orderItem)
        {
            return new OrderItemDTO
            {
                Id = orderItem.Id,
                OrderId = orderItem.OrderId,
                MenuItemId = orderItem.MenuItemId,
                MenuItemName = orderItem.MenuItemName,
                MenuItemPrice = orderItem.MenuItemPrice,
                MenuItemDescription = orderItem.MenuItemDescription,
                Quantity = orderItem.Quantity,
            };
        }
    }
}
