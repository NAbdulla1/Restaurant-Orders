using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Data.Repositories
{
    public interface IOrderItemRepository
    {
        OrderItem Add(OrderItem item);
        void DeleteMany(List<OrderItem> deleteExistingOrderItems);
        IQueryable<OrderItem> SearchInName(string searchTerm);
        OrderItem Update(OrderItem item);
    }

    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly RestaurantContext _dbContext;

        public OrderItemRepository(RestaurantContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void DeleteMany(List<OrderItem> deleteExistingOrderItems)
        {
            _dbContext.RemoveRange(deleteExistingOrderItems);
        }

        public IQueryable<OrderItem> SearchInName(string searchTerm)
        {
            return _dbContext.OrderItems.Where(
                orderItems => orderItems.MenuItemName != null && orderItems.MenuItemName.Contains(searchTerm));
        }

        public OrderItem Add(OrderItem item)
        {
            _dbContext.OrderItems.Add(item);
            return item;
        }

        public OrderItem Update(OrderItem item)
        {
            var entry = _dbContext.OrderItems.Attach(item);
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            return item;
        }
    }
}
