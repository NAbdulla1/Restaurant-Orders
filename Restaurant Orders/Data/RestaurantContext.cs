using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Data.Entities;

namespace Restaurant_Orders.Data
{
    public class RestaurantContext : DbContext
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> optionsBuilder) : base(optionsBuilder) { }

        public DbSet<User> Users { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildMenuItemModelConstraints(modelBuilder);
        }

        private static void BuildMenuItemModelConstraints(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MenuItem>()
                .ToTable("menu_items")
                .HasKey(x => x.Id);

            modelBuilder.Entity<MenuItem>()
                .Property(x => x.Id)
                .HasColumnName("id");

            modelBuilder.Entity<MenuItem>()
                .Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<MenuItem>()
                .Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsRequired(false);

            modelBuilder.Entity<MenuItem>()
                .Property(x => x.Price)
                .HasPrecision(6, 2)
                .HasColumnName("price")
                .IsRequired();
        }
    }
}
