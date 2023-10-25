using Restaurant_Orders.Data.Entities;
using Restaurant_Orders.Models.DTOs;
using System.Linq.Expressions;

namespace Restaurant_Orders.Services
{
    public interface IMenuItemService
    {
        IQueryable<MenuItem> PrepareIndexQuery(IQueryable<MenuItem> query, IndexingDTO indexData);
    }

    public class MenuItemService : IMenuItemService
    {
        public IQueryable<MenuItem> PrepareIndexQuery(IQueryable<MenuItem> query, IndexingDTO indexData)
        {
            if (indexData.SearchBy != null)
            {
                query = AddSearchQuery(query, indexData.SearchBy);
            }

            if (indexData.SortBy != null)
            {
                query = AddSortQuery(query, indexData.SortBy, indexData.SortOrder);
            }

            return query;
        }

        private static IQueryable<MenuItem> AddSearchQuery(IQueryable<MenuItem> result, string searchTerm)
        {
            result = result.Where(
                item =>
                    item.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    (
                        item.Description != null && item.Description.ToLower().Contains(searchTerm.ToLower())
                    )
                );

            return result;
        }

        private static IQueryable<MenuItem> AddSortQuery(IQueryable<MenuItem> result, string sortColumn, string? sortDirection)
        {
            if (sortDirection == null || sortDirection == "asc")
            {
                result = result.OrderBy(GetOrderExpression(sortColumn));
            }
            else
            {
                result = result.OrderByDescending(GetOrderExpression(sortColumn));
            }

            return result;
        }

        private static Expression<Func<MenuItem, object?>> GetOrderExpression(string sortColumn)
        {
            switch (sortColumn)
            {
                case "name":
                    return item => item.Name;
                case "description":
                    return item => item.Description;
                case "price":
                    return item => item.Price;
                default:
                    return item => item.Id;
            }
        }
    }
}
