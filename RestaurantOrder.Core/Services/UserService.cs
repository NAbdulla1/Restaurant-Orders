using Microsoft.AspNetCore.Http;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Core.Extensions;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;
using System.Security.Claims;
using System.Text.Json;

namespace Restaurant_Orders.Services
{
    public interface IUserService
    {
        Task<UserDTO> CreateCustomer(CustomerRegisterDTO customer);
        Task<UserDTO> UpdateUser(long userId, UserUpdateDTO userUpdateDTO);
        UserDTO GetCurrentAuthenticatedUser(HttpContext httpContext);
        Task<AccessTokenDTO> SignInUser(LoginPayloadDTO loginPayload);
        Task<UserDTO> GetUser(long userId);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository userRepository, IPasswordService passwordService, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
            _passwordService = passwordService;
        }

        public async Task<UserDTO> CreateCustomer(CustomerRegisterDTO customer)
        {
            var existingUser = await _userRepository.GetByEmail(customer.Email);
            if (existingUser != null)
            {
                throw new CustomerAlreadyExistsException($"Another user already exists with the given email: '{customer.Email}'.");
            }

            var user = new User
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Password = _passwordService.HashPassword(customer.Password),
                UserType = UserType.Customer,
            };

            user = _userRepository.Add(user);
            await _userRepository.Commit();

            return user.ToUserDTO();
        }

        public async Task<UserDTO> UpdateUser(long userId, UserUpdateDTO userUpdateDTO)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            user.FirstName = userUpdateDTO.FirstName;
            user.LastName = userUpdateDTO.LastName;
            if (userUpdateDTO.NewPassword != null)
            {
                user.Password = _passwordService.HashPassword(userUpdateDTO.NewPassword);
            }

            await _userRepository.Commit();

            return user.ToUserDTO();
        }

        public async Task<AccessTokenDTO> SignInUser(LoginPayloadDTO loginPayload)
        {
            var user = await _userRepository.GetByEmail(loginPayload.Email);

            if (user == null || !_passwordService.VerifyPassword(user.Password, loginPayload.Password))
            {
                throw new UnauthenticatedException("User authentication failed.");
            }

            var token = _tokenService.CreateToken(user);

            return new AccessTokenDTO { AccessToken = token };
        }

        public UserDTO GetCurrentAuthenticatedUser(HttpContext httpContext)
        {
            if (!httpContext.User.HasClaim(c => c.Type == ClaimTypes.UserData))
            {
                throw new UnauthenticatedException("User authentication failed.");
            }

            var userData = httpContext.User.FindFirstValue(ClaimTypes.UserData);

            var userDto = JsonSerializer.Deserialize<UserDTO>(userData)!;

            return userDto;
        }

        public async Task<UserDTO> GetUser(long userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            return user.ToUserDTO();
        }
    }
}
