using Microsoft.Extensions.DependencyInjection;
using RestaurantOrder.Data.Repositories;

namespace RestaurantOrder.Data.Extensions
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterDataServices(this IServiceCollection services)
        {
            return services
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IOrderRepository, OrderRepository>()
                .AddScoped<IMenuItemRepository, MenuItemRepository>()
                .AddScoped<IOrderItemRepository, OrderItemRepository>()
                .AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
