using Microsoft.EntityFrameworkCore;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Models.DTOs;

namespace RestaurantOrder.Data.Repositories
{
    public interface IRepository<TModel> where TModel : ModelBase
    {
        Task<TModel?> GetByIdAsync(long id);
        Task<QueryResult<TModel>> GetAllAsync(QueryDetailsDTO<TModel> queryDetails);

        void Add(TModel model);
        void AddMany(IEnumerable<TModel> models);

        void Delete(TModel model);
        void DeleteRange(IEnumerable<TModel> models);

        Task<bool> IsItemExistsAsync(long id);
    }

    public class Repository<TModel> : IRepository<TModel> where TModel : ModelBase
    {
        protected DbContext _dbContext { get; set; }

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(TModel model)
        {
            _dbContext.Set<TModel>().Add(model);
        }

        public void AddMany(IEnumerable<TModel> models)
        {
            _dbContext.Set<TModel>().AddRange(models);
        }

        public void Delete(TModel model)
        {
            _dbContext.Set<TModel>().Remove(model);
        }

        public void DeleteRange(IEnumerable<TModel> models)
        {
            _dbContext.Set<TModel>().RemoveRange(models);
        }

        public async Task<QueryResult<TModel>> GetAllAsync(QueryDetailsDTO<TModel> queryDetails)
        {
            var query = queryDetails.WhereQueries.Aggregate(
                _dbContext.Set<TModel>().AsQueryable(),
                (current, where) =>  current.Where(where));

            query = queryDetails.SortOrder == "desc" ?
                query.OrderByDescending(queryDetails.OrderingExpr) :
                query.OrderBy(queryDetails.OrderingExpr);

            int total = await query.CountAsync();
            int skip = (queryDetails.Page - 1) * queryDetails.PageSize;

            var result = query.Skip(skip)
                    .Take(queryDetails.PageSize)
                    .AsEnumerable();

            return new QueryResult<TModel>
            {
                Total = total,
                Data = result
            };
        }

        public async Task<TModel?> GetByIdAsync(long id)
        {
            return await _dbContext.Set<TModel>().FindAsync(id);
        }

        public async Task<bool> IsItemExistsAsync(long id)
        {
            return await _dbContext.Set<TModel>().AnyAsync(item => item.Id == id);
        }
    }
}
