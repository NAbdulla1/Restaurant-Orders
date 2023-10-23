using Restaurant_Orders.Data;
using Restaurant_Orders.Data.Entities;
using Restaurant_Orders.Enums;
using Restaurant_Orders.Models;
using Restaurant_Orders.Services;

namespace Restaurant_Orders.Extensions
{
    public static class AdminSeeder
    {
        public static void SeedAdmin(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var passwordService = services.GetRequiredService<IPasswordService>();
                var restaurantContext = services.GetRequiredService<RestaurantContext>();
                var ownerInfo = app.Configuration.GetRequiredSection(OwnerConfigData.ConfigSectionName).Get<OwnerConfigData>();

                if (restaurantContext.Users.Where(user => user.UserType == UserType.RestaurantOwner).Any())
                {
                    return;
                }

                restaurantContext.Users.Add(new User
                {
                    FirstName = ownerInfo.FirstName,
                    LastName = ownerInfo.LastName,
                    Email = ownerInfo.Email,
                    Password = passwordService.HashPassword(ownerInfo.Password),
                    UserType = UserType.RestaurantOwner
                });

                restaurantContext.SaveChanges();
            }
        }
    }
}
