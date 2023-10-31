using Microsoft.AspNetCore.Authorization;
using Restaurant_Orders.Authorizations;
using Restaurant_Orders.Models;
using Restaurant_Orders.Services;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;

namespace Restaurant_Orders.Infrastructure
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            return services
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IMenuItemRepository, MenuItemRepository>()
                .AddScoped<IOrderItemRepository,  OrderItemRepository>()
                .AddScoped<IOrderRepository, OrderRepository>()
                .AddScoped<IAuthorizationHandler, OwnProfileModifyHandler>()
                .AddScoped<IPasswordService, PasswordService>()
                .AddScoped<ITokenService, JsonWebTokenService>()
                .AddScoped<IUserService, UserService>()
                .AddScoped<IMenuItemService, MenuItemService>()
                .AddScoped<IPaginationService<MenuItem>, PaginationService<MenuItem>>()
                .AddScoped<IPaginationService<Order>, PaginationService<Order>>()
                .AddScoped<IOrderService, OrderService>();
        }
    }
}
