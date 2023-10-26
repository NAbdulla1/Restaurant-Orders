using System.Reflection;

namespace Restaurant_Orders.Extensions
{
    public static class TypeExtensions
    {
        public static PropertyInfo? GetPropertyInfo(this Type type, string? name)
        {
            if (name == null) return null;
            return type
                .GetProperties()
                .FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
        }

        public static bool FieldExists(this Type type, string? name)
        {
            return type.GetPropertyInfo(name) != null;
        }
    }
}
