using Microsoft.EntityFrameworkCore;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Data.Repositories
{
    public interface IOrderRepository
    {
        void Add(Order order);
        Task Commit();
        void Delete(Order order, Guid version);
        IQueryable<Order> GetAll();
        Task<Order?> GetById(long id);
        Task<bool> OrderExists(long id);
        void UpdateOrder(Order order, Guid? version);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly RestaurantContext _dbContext;

        public OrderRepository(RestaurantContext dbContext)
        {
            _dbContext = dbContext;

        }

        public void Add(Order order)
        {
            _dbContext.Orders.Add(order);
        }

        public async Task Commit()
        {
            await _dbContext.SaveChangesAsync();
        }

        public void Delete(Order order, Guid version)
        {
            _dbContext.Entry(order).State = EntityState.Deleted;
            _dbContext.Entry(order).Property("Version").OriginalValue = version;
        }

        public IQueryable<Order> GetAll()
        {
            return _dbContext.Orders.Where(order => true);
        }

        public async Task<Order?> GetById(long id)
        {
            return await _dbContext.Orders.FirstOrDefaultAsync(order => order.Id == id);
        }

        public async Task<bool> OrderExists(long id)
        {
            return await _dbContext.Orders.AnyAsync(order => order.Id == id);
        }

        public void UpdateOrder(Order order, Guid? version)
        {
            if(version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            _dbContext.Entry(order).State = EntityState.Modified;
            _dbContext.Entry(order).Property("Version").OriginalValue = version.Value;
        }
    }
}
