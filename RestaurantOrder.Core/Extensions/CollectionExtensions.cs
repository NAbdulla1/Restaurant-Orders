namespace RestaurantOrder.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static Dictionary<T, int> CountFrequency<T>(this ICollection<T> items) where T : notnull
        {
            return items
                .GroupBy(id => id)
                .ToDictionary(group => group.Key, g => g.Count());
        }
    }
}
