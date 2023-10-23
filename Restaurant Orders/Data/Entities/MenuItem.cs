namespace Restaurant_Orders.Data.Entities
{
    public class MenuItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
