using Restaurant_Orders.Exceptions;
using Restaurant_Orders.Models;
using Restaurant_Orders.Models.DTOs;
using System.Security.Claims;
using System.Text.Json;

namespace Restaurant_Orders.Services
{
    public interface IUserService
    {
        string SignInUser(User user, string password);
        User PrepareCustomer(User customer);
        void PrepareUserUpdate(User user, UserUpdateDTO userUpdateDTO);
        UserDTO GetCurrentUser(HttpContext httpContext);
    }

    public class UserService : IUserService
    {
        public UserService(IPasswordService passwordService, ITokenService tokenService)
        {
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public User PrepareCustomer(User customer)
        {
            customer.UserType = UserType.Customer;
            customer.Password = _passwordService.HashPassword(customer.Password);

            return customer;
        }

        public void PrepareUserUpdate(User user, UserUpdateDTO userUpdateDTO)
        {
            user.FirstName = userUpdateDTO.FirstName;
            user.LastName = userUpdateDTO.LastName;
            user.Password = _passwordService.HashPassword(userUpdateDTO.Password);
        }

        public string SignInUser(User user, string password)
        {
            if (!_passwordService.VerifyPassword(user.Password, password))
            {
                throw new UnauthenticatedException("User authentication failed.");
            }

            return _tokenService.CreateToken(user);
        }

        public UserDTO GetCurrentUser(HttpContext httpContext)
        {
            if(!httpContext.User.HasClaim(c => c.Type == ClaimTypes.UserData))
            {
                throw new UnauthenticatedException("User authentication failed.");
            }

            var userData = httpContext.User.FindFirstValue(ClaimTypes.UserData);

            var userDto = JsonSerializer.Deserialize<UserDTO>(userData)!;

            return userDto;
        }
    }
}
