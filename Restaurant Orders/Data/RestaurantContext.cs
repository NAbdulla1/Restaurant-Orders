using Microsoft.EntityFrameworkCore;

namespace Restaurant_Orders.Data
{
    public class RestaurantContext : DbContext
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> optionsBuilder) : base(optionsBuilder)
        { }
    }
}
