using Microsoft.AspNetCore.Mvc;
using Restaurant_Orders.Data.Entities;
using Restaurant_Orders.Exceptions;

namespace Restaurant_Orders.Services
{
    public interface IUserService
    {
        string SignInUser(User user, string password);
        User PrepareCustomer(User customer);
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
            customer.UserType = Enums.UserType.Customer;
            customer.Password = _passwordService.HashPassword(customer.Password);

            return customer;
        }

        public string SignInUser(User user, string password)
        {
            if(!_passwordService.VerifyPassword(user.Password, password))
            {
                throw new UnauthenticatedException("User authentication failed.");
            }

            return _tokenService.CreateToken(user);
        }
    }
}
