using RestaurantOrder.Data.Repositories;

namespace RestaurantOrder.Data
{
    public interface IUnitOfWork
    {
        IMenuItemRepository MenuItems { get; }
        IOrderItemRepository OrderItems { get; }
        IOrderRepository Orders { get; }
        IUserRepository Users { get; }

        Task<int> Commit();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly RestaurantContext _restaurantContext;

        public IUserRepository Users { get; private set; }
        public IOrderRepository Orders { get; private set; }
        public IMenuItemRepository MenuItems { get; private set; }
        public IOrderItemRepository OrderItems { get; private set; }

        public UnitOfWork(RestaurantContext restaurantContext)
        {
            _restaurantContext = restaurantContext;

            Users = new UserRepository(restaurantContext);
            Orders = new OrderRepository(restaurantContext);
            MenuItems = new MenuItemRepository(restaurantContext);
            OrderItems = new OrderItemRepository(restaurantContext);
        }

        public async Task<int> Commit()
        {
            return await _restaurantContext.SaveChangesAsync();
        }
    }
}
