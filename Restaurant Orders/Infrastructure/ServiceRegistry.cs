using Microsoft.AspNetCore.Authorization;
using Restaurant_Orders.Authorizations;
using Restaurant_Orders.Services;

namespace Restaurant_Orders.Infrastructure
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IAuthorizationHandler, OwnProfileModifyHandler>()
                .AddScoped<IPasswordService, PasswordService>()
                .AddScoped<ITokenService, JsonWebTokenService>()
                .AddScoped<IUserService, UserService>();
        }
    }
}
