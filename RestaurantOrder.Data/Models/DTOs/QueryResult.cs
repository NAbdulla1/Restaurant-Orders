namespace RestaurantOrder.Data.Models.DTOs
{
    public class QueryResult<T>
    {
        public long Total { get; set; }
        public IEnumerable<T> Data { get; set;}
    }
}
