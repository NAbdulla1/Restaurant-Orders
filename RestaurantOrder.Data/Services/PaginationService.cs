using Microsoft.EntityFrameworkCore;
using RestaurantOrder.Data.Models.DTOs;

namespace RestaurantOrder.Data.Services
{
    public interface IPaginationService<T> where T : class
    {
        Task<QueryResult<T>> Paginate(IQueryable<T> query, int page, int pageSize);
    }

    public class PaginationService<T> : IPaginationService<T> where T : class
    {
        public async Task<QueryResult<T>> Paginate(IQueryable<T> query, int page, int pageSize)
        {
            int total = await query.CountAsync();
            int skip = (page - 1) * pageSize;

            var result = query.Skip(skip)
                    .Take(pageSize)
                    .AsEnumerable();

            return new QueryResult<T>
            {
                Total = total,
                Data = result
            };
        }
    }
}
