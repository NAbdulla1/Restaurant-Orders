using Microsoft.AspNetCore.Authorization;
using Restaurant_Orders.Authorizations;
using RestaurantOrder.Core.Extensions;

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
