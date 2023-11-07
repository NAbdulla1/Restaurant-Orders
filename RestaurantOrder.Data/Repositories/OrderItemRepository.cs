using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Data.Repositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        IQueryable<OrderItem> SearchInName(string searchTerm);
    }

    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(RestaurantContext dbContext) : base(dbContext) { }

        public IQueryable<OrderItem> SearchInName(string searchTerm)
        {
            return Context.OrderItems.Where(
                orderItems => orderItems.MenuItemName != null && orderItems.MenuItemName.Contains(searchTerm));
        }

        private RestaurantContext Context => (RestaurantContext)_dbContext;
    }
}
