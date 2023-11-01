using Microsoft.Extensions.DependencyInjection;
using Restaurant_Orders.Services;
using RestaurantOrder.Data.Extensions;

namespace RestaurantOrder.Core.Extensions
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterCoreServices(this IServiceCollection services)
        {
            return services.RegisterDataServices()
                .AddScoped<IPasswordService, PasswordService>()
                .AddScoped<ITokenService, JsonWebTokenService>()
                .AddScoped<IUserService, UserService>()
                .AddScoped<IMenuItemService, MenuItemService>()
                .AddScoped<IOrderService, OrderService>();
        }
    }
}
