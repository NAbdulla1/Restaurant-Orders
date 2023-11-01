using Microsoft.EntityFrameworkCore;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Models.DTOs;
using RestaurantOrder.Data.Services;

namespace RestaurantOrder.Data.Repositories
{
    public interface IMenuItemRepository
    {
        MenuItem Add(MenuItem item);
        Task<int> Commit();
        void Delete(MenuItem menuItem);
        Task<QueryResult<MenuItem>> GetAll(QueryDetailsDTO<MenuItem> queryDetails);
        Task<MenuItem?> GetById(long id);
        Task<IEnumerable<MenuItem>> GetByIds(List<long> ids);
        Task<bool> MenuItemExists(long id);
        void UpdateMenuItem(MenuItem menuItem);
    }

    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly RestaurantContext _dbContext;
        private readonly IPaginationService<MenuItem> _paginationService;

        public MenuItemRepository(RestaurantContext dbContext, IPaginationService<MenuItem> paginationService)
        {
            _dbContext = dbContext;
            _paginationService = paginationService;
        }

        public async Task<QueryResult<MenuItem>> GetAll(QueryDetailsDTO<MenuItem> queryDetails)
        {
            var query = _dbContext.MenuItems.AsQueryable();
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

        public MenuItem Add(MenuItem item)
        {
            _dbContext.MenuItems.Add(item);
            return item;
        }

        public async Task<int> Commit()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Delete(MenuItem menuItem)
        {
            _dbContext.MenuItems.Remove(menuItem);
        }

        public async Task<MenuItem?> GetById(long id)
        {
            return await _dbContext.MenuItems.FirstOrDefaultAsync(menuItem => menuItem.Id == id);
        }

        public async Task<bool> MenuItemExists(long id)
        {
            return await _dbContext.MenuItems.AnyAsync(menuItem => menuItem.Id == id);
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            _dbContext.MenuItems.Attach(menuItem);
            _dbContext.Entry(menuItem).State = EntityState.Modified;
        }

        public async Task<IEnumerable<MenuItem>> GetByIds(List<long> ids)
        {
            return await _dbContext.MenuItems.Where(menuItem => ids.Contains(menuItem.Id)).ToListAsync();
        }
    }
}
