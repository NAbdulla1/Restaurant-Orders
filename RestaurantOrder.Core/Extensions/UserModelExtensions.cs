using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.Core.Extensions
{
    public static class UserModelExtensions
    {
        public static UserDTO ToUserDTO(this User user)
        {
            return new UserDTO()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserType = user.UserType.ToString(),
            };
        }
    }
}
