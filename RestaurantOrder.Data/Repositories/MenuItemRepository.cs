using Microsoft.EntityFrameworkCore;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Data.Repositories
{
    public interface IMenuItemRepository
    {
        void Add(MenuItem item);
        Task Commit();
        void Delete(MenuItem menuItem);
        IQueryable<MenuItem> GetAll();
        Task<MenuItem?> GetById(long id);
        Task<IEnumerable<MenuItem>> GetByIds(List<long> ids);
        Task<bool> MenuItemExists(long id);
        void UpdateMenuItem(MenuItem menuItem);
    }

    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly RestaurantContext _dbContext;

        public MenuItemRepository(RestaurantContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<MenuItem> GetAll()
        {
            return _dbContext.MenuItems.Where(item => true);
        }

        public void Add(MenuItem item)
        {
            _dbContext.MenuItems.Add(item);
        }

        public async Task Commit()
        {
            await _dbContext.SaveChangesAsync();
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
