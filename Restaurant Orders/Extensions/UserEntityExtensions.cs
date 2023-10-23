using Restaurant_Orders.Data.Entities;
using Restaurant_Orders.Models.DTOs;

namespace Restaurant_Orders.Extensions
{
    public static class UserEntityExtensions
    {
        public static UserDTO ToUserDTO(this User user)
        {
            return new UserDTO()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }
    }
}
