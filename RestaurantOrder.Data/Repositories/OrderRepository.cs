using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Data.Repositories
{
    public interface IOrderRepository : IRepository<Order> { }

    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(RestaurantContext dbContext) : base(dbContext) { }
    }
}
