using Microsoft.Extensions.DependencyInjection;
using Restaurant_Orders.Services;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Data;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;

namespace RestaurantOrder.Core.Data
{
    public class DataSeeder
    {
        public static async Task SeedAdmin(IServiceProvider services, OwnerConfigData ownerInfo)
        {
            var passwordService = services.GetRequiredService<IPasswordService>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            if (await unitOfWork.Users.HasAnyAdmin())
            {
                return;
            }

            unitOfWork.Users.Add(new User
            {
                FirstName = ownerInfo.FirstName,
                LastName = ownerInfo.LastName,
                Email = ownerInfo.Email,
                Password = passwordService.HashPassword(ownerInfo.Password),
                UserType = UserType.RestaurantOwner
            });

            await unitOfWork.Commit();
        }
    }
}
