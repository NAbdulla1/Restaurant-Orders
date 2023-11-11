using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace RestaurantOrder.Data.Models.DTOs
{
    public class QueryDetailsDTO<T> where T : ModelBase
    {
        private List<Expression<Func<T, bool>>> _whereQueries;

        public QueryDetailsDTO(
            Expression<Func<T, object?>> orderExpr,
            bool orderDescending,
            int page,
            int pageSize)
        {
            Page = page;
            PageSize = pageSize;
            DescendingOrder = orderDescending;
            OrderingExpr = orderExpr;
            _whereQueries = new List<Expression<Func<T, bool>>>();
        }

        public int Page { get; private set; }

        public int PageSize { get; private set; }

        public ReadOnlyCollection<Expression<Func<T, bool>>> WhereQueries
        {
            get
            {
                return _whereQueries.AsReadOnly();
            }
        }

        public Expression<Func<T, object?>> OrderingExpr { get; private set; } = item => item.Id;

        public bool DescendingOrder { get; private set; } = false;

        public void AddQuery(Expression<Func<T, bool>> query)
        {
            _whereQueries.Add(query);
        }
    }
}
