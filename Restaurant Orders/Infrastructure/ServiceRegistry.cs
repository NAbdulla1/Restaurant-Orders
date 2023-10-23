using Restaurant_Orders.Services;

namespace Restaurant_Orders.Infrastructure
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {

            return services
                .AddScoped<IPasswordService, PasswordService>()
                .AddScoped<ITokenService, JsonWebTokenService>()
                .AddScoped<IUserService, UserService>();
        }
    }
}
