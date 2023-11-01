using Microsoft.Extensions.DependencyInjection;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;
using RestaurantOrder.Data.Services;

namespace RestaurantOrder.Data.Extensions
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterDataServices(this IServiceCollection services)
        {
            return services.AddScoped<IPaginationService<MenuItem>, PaginationService<MenuItem>>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IMenuItemRepository, MenuItemRepository>()
                .AddScoped<IOrderItemRepository, OrderItemRepository>()
                .AddScoped<IOrderRepository, OrderRepository>();
        }
    }
}
