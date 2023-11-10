﻿namespace RestaurantOrder.Data.Models
{
    public class Order : ModelBase
    {
        public long CustomerId { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public User Customer { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        CREATED, PROCESSING, BILLED, CANCELED, CLOSED
    }
}
