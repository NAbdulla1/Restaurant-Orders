namespace Restaurant_Orders.Models
{
    public class Order
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid Version { get; set; }

        public User Customer { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        CREATED, PROCESSING, BILLED, CANCELED, CLOSED
    }
}
