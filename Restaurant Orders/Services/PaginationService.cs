using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Models.DTOs;

namespace Restaurant_Orders.Services
{
    public interface IPaginationService<T> where T:class
    {
        Task<PagedData<T>> Paginate(IQueryable<T> query, IndexingDTO indexData);
    }

    public class PaginationService<T> : IPaginationService<T> where T:class
    {
        private readonly int _defaultPageSize;

        public PaginationService(IConfiguration configuration)
        {
            _defaultPageSize = configuration.GetValue<int>("DefaultPageSize");
        }

        public async Task<PagedData<T>> Paginate(IQueryable<T> query, IndexingDTO indexData)
        {
            int total = await query.CountAsync();

            int take = indexData.PageSize ?? _defaultPageSize;
            int skip = (indexData.Page - 1) * take;

            var pageData = query.Skip(skip)
                    .Take(take)
                    .AsAsyncEnumerable();

            var hasPreviousPage = indexData.Page > 1;

            var totalPages = Math.Ceiling((decimal)total / take);
            var hasNextPage = indexData.Page < totalPages;

            return new PagedData<T>
            {
                Page = indexData.Page,
                PageSize = take,
                Total = total,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
                PageData = pageData
            };
        }
    }
}
