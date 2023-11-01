using Microsoft.AspNetCore.Authorization;
using Restaurant_Orders.Authorizations;
using Restaurant_Orders.Services;
using RestaurantOrder.Core.Extensions;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;

namespace Restaurant_Orders.Infrastructure
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            return services
                .RegisterCoreServices()
                .AddScoped<IAuthorizationHandler, OwnProfileModifyHandler>();
        }
    }
}
