using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Core.DTOs
{
    public class OrderItemDTO
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long? MenuItemId { get; set; }
        public string? MenuItemName { get; set; }
        public string? MenuItemDescription { get; set; }
        public decimal MenuItemPrice { get; set; }
        public int Quantity { get; set; }

        public OrderItem ToOrderItem()
        {
            return new OrderItem
            {
                Id = Id,
                OrderId = OrderId,
                MenuItemId = MenuItemId,
                MenuItemName = MenuItemName,
                MenuItemDescription = MenuItemDescription,
                MenuItemPrice = MenuItemPrice,
                Quantity = Quantity
            };
        }
    }
}
