namespace RestaurantOrder.Data.Models
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long? MenuItemId { get; set; }
        public string? MenuItemName { get; set; }
        public string? MenuItemDescription { get; set; }
        public decimal MenuItemPrice { get; set; }
        public int Quantity { get; set; } = 1;

        public Order Order { get; set; } = null!;
        public MenuItem? MenuItem { get; set; }
    }
}
