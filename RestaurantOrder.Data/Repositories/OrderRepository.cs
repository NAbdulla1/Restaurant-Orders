using Microsoft.EntityFrameworkCore;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Models.DTOs;
using RestaurantOrder.Data.Services;

namespace RestaurantOrder.Data.Repositories
{
    public interface IOrderRepository
    {
        Order Add(Order order);
        Task Commit();
        void Delete(long id, Guid version);
        Task<QueryResult<Order>> GetAll(QueryDetailsDTO<Order> queryDetails);
        Task<Order?> GetById(long id);
        Task<bool> OrderExists(long id);
        Order UpdateOrder(Order order, Guid originalVersion);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly RestaurantContext _dbContext;
        private readonly IPaginationService<Order> _paginationService;

        public OrderRepository(RestaurantContext dbContext, IPaginationService<Order> paginationService)
        {
            _dbContext = dbContext;
            _paginationService = paginationService;
        }

        public Order Add(Order order)
        {
            _dbContext.Orders.Add(order);
            return order;
        }

        public async Task Commit()
        {
            await _dbContext.SaveChangesAsync();
        }

        public void Delete(long id, Guid version)
        {
            var entry = _dbContext.Entry(new Order { Id = id });
            entry.State = EntityState.Deleted;
            entry.Property("Version").OriginalValue = version;
        }

        public async Task<QueryResult<Order>> GetAll(QueryDetailsDTO<Order> queryDetails)
        {
            var query = _dbContext.Orders.AsQueryable();
            foreach (var whereQuery in queryDetails.WhereQueries)
            {
                query = query.Where(whereQuery);
            }

            if (queryDetails.OrderingExpr != null)
            {
                query = queryDetails.SortOrder == "asc" ? query.OrderBy(queryDetails.OrderingExpr) : query.OrderByDescending(queryDetails.OrderingExpr);
            }

            return await _paginationService.Paginate(query, queryDetails.Page, queryDetails.PageSize);
        }

        public async Task<Order?> GetById(long id)
        {
            return await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(order => order.Id == id);
        }

        public async Task<bool> OrderExists(long id)
        {
            return await _dbContext.Orders.AnyAsync(order => order.Id == id);
        }

        public Order UpdateOrder(Order order, Guid originalVersion)
        {
            var updatedOrder = new Order
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                CustomerId = order.CustomerId,
                Status = order.Status,
                Total = order.Total,
                Version = Guid.NewGuid(),
                OrderItems = order.OrderItems
            };

            var entry = _dbContext.Entry(updatedOrder);
            
            entry.State = EntityState.Modified;
            entry.Property("Version").OriginalValue = originalVersion;

            return updatedOrder;
        }
    }
}
