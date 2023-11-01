using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace RestaurantOrder.Data.Models.DTOs
{
    public class QueryDetailsDTO<T>
    {
        public int Page {  get; private set; }

        public int PageSize { get; private set; }

        public List<Expression<Func<T, bool>>> WhereQueries { get; private set; }

        public Expression<Func<T, object?>>? OrderingExpr { get; set; }

        public string SortOrder { get; set; }

        public QueryDetailsDTO(int page, int pageSize)
        {
            WhereQueries = new();
            Page = page;
            PageSize = pageSize;
            SortOrder = "asc";
        }
    }
}
