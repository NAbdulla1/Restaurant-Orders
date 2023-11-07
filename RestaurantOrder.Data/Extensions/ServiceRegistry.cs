using Microsoft.Extensions.DependencyInjection;

namespace RestaurantOrder.Data.Extensions
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterDataServices(this IServiceCollection services)
        {
            return services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
