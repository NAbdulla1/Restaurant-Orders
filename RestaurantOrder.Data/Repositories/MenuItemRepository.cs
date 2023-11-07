using Microsoft.EntityFrameworkCore;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Data.Repositories
{
    public interface IMenuItemRepository : IRepository<MenuItem>
    {
        Task<IEnumerable<MenuItem>> GetByIdsAsync(List<long> ids);
    }

    public class MenuItemRepository : Repository<MenuItem>, IMenuItemRepository
    {
        public MenuItemRepository(RestaurantContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<MenuItem>> GetByIdsAsync(List<long> ids)
        {
            return await Context.MenuItems.Where(menuItem => ids.Contains(menuItem.Id)).ToListAsync();
        }


        private RestaurantContext Context => (RestaurantContext)_dbContext;
    }
}
