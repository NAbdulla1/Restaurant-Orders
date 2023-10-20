using Microsoft.AspNetCore.Identity;
using Restaurant_Orders.Data.Entities;

namespace Restaurant_Orders.Services
{
    public interface IUserService
    {
        bool LoginUser(User user, string password);
        User PrepareCustomer(User customer);
    }

    public class UserService : IUserService
    {
        public UserService(IPasswordService passwordService)
        {
            this._passwordService = passwordService;
        }

        private readonly IPasswordService _passwordService;

        public User PrepareCustomer(User customer)
        {
            customer.UserType = Enums.UserType.Customer;
            customer.Password = _passwordService.HashPassword(customer.Password);

            return customer;
        }

        public bool LoginUser(User? user, string password)
        {
            return _passwordService.VerifyPassword(user.Password, password);//TODO create and return actual access token
        }
    }
}
