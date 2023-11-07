using Microsoft.EntityFrameworkCore;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmail(string email);
        Task<bool> HasAnyAdmin();
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(RestaurantContext dbContext) : base(dbContext) { }

        public async Task<User?> GetByEmail(string email)
        {
            return await Context.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<bool> HasAnyAdmin()
        {
            return await Context.Users.AnyAsync(user => user.UserType == UserType.RestaurantOwner);
        }

        private RestaurantContext Context => (RestaurantContext)_dbContext;
    }
}
