using Restaurant_Orders.Services;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;

namespace Restaurant_Orders.Data
{
    public class SeedData
    {
        public static async Task SeedAdmin(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var passwordService = services.GetRequiredService<IPasswordService>();
                var userRepository = services.GetRequiredService<IUserRepository>();
                var ownerInfo = app.Configuration.GetRequiredSection(OwnerConfigData.ConfigSectionName).Get<OwnerConfigData>();

                if (await userRepository.HasAnyAdmin())
                {
                    return;
                }

                userRepository.Add(new User
                {
                    FirstName = ownerInfo.FirstName,
                    LastName = ownerInfo.LastName,
                    Email = ownerInfo.Email,
                    Password = passwordService.HashPassword(ownerInfo.Password),
                    UserType = UserType.RestaurantOwner
                });

                await userRepository.Commit();
            }
        }
    }
}
