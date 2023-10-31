using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Data.Repositories
{
    public interface IOrderItemRepository
    {
        void RemoveMany(List<OrderItem> deleteExistingOrderItems);
        IQueryable<OrderItem> SearchInName(string searchTerm);
    }

    public class OrderItemRepository: IOrderItemRepository
    {
        private readonly RestaurantContext _dbContext;

        public OrderItemRepository(RestaurantContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void RemoveMany(List<OrderItem> deleteExistingOrderItems)
        {
            _dbContext.RemoveRange(deleteExistingOrderItems);
        }

        public IQueryable<OrderItem> SearchInName(string searchTerm)
        {
            return _dbContext.OrderItems.Where(
                orderItems => orderItems.MenuItemName != null && orderItems.MenuItemName.Contains(searchTerm));
        }
    }
}
