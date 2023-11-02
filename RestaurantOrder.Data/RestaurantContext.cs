using Microsoft.EntityFrameworkCore;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Data
{
    public class RestaurantContext : DbContext
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> optionsBuilder) : base(optionsBuilder) { }

        public DbSet<User> Users { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildUserModelConstraints(modelBuilder);
            BuildMenuItemModelConstraints(modelBuilder);
            BuildOrderModelConstraints(modelBuilder);
            BuildOrderItemModelConstraints(modelBuilder);
        }

        private void BuildUserModelConstraints(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(user => user.Orders)
                .WithOne(order => order.Customer)
                .HasForeignKey(order => order.CustomerId)
                .IsRequired();
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

            modelBuilder.Entity<MenuItem>()
                .HasMany(menuItem => menuItem.OrderItems)
                .WithOne(orderItem => orderItem.MenuItem)
                .HasForeignKey(orderItem => orderItem.MenuItemId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }

        private void BuildOrderModelConstraints(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .ToTable("orders")
                .HasKey(order => order.Id);

            modelBuilder.Entity<Order>()
                .Property(order => order.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Order>()
                .Property(order => order.CustomerId)
                .HasColumnName("customer_id")
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(order => order.Total)
                .HasColumnName("total")
                .HasPrecision(10, 2)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(order => order.Status)
                .HasColumnName("status")
                .HasDefaultValue(OrderStatus.CREATED)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(order => order.CreatedAt)
                .HasDefaultValueSql("getdate()")
                .HasColumnName("created_at")
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(order => order.Version)
                .IsConcurrencyToken();

            modelBuilder.Entity<Order>()
                .HasMany(order => order.OrderItems)
                .WithOne(orderItem => orderItem.Order)
                .HasForeignKey(orderItem => orderItem.OrderId)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Navigation(order => order.OrderItems)
                .AutoInclude();
        }

        private void BuildOrderItemModelConstraints(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>()
                .ToTable("order_items")
                .HasKey(x => x.Id);

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.Id)
                .HasColumnName("id");

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.MenuItemId)
                .HasColumnName("menu_item_id")
                .IsRequired(false);

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.MenuItemName)
                .HasColumnName("menu_item_name")
                .HasMaxLength(255)
                .IsRequired(false);

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.MenuItemDescription)
                .HasColumnName("menu_item_description")
                .HasMaxLength(2000)
                .IsRequired(false);

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.MenuItemPrice)
                .HasPrecision(6, 2)
                .HasColumnName("menu_item_price")
                .IsRequired();

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.Quantity)
                .HasColumnName("quantity")
                .HasDefaultValue(1)
                .IsRequired();
        }
    }
}
