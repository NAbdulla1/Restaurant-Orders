using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Data.Entities;

namespace Restaurant_Orders.Data
{
    public class RestaurantContext : DbContext
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> optionsBuilder) : base(optionsBuilder)
        { }

        public DbSet<User> Users { get; set; }
    }
}
