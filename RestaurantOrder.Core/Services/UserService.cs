using Microsoft.AspNetCore.Http;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Core.Extensions;
using RestaurantOrder.Data;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public UserService(IUnitOfWork unitOfWork, IPasswordService passwordService, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
        }

        public async Task<UserDTO> CreateCustomer(CustomerRegisterDTO customer)
        {
            var existingUser = await _unitOfWork.Users.GetByEmail(customer.Email);
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

            _unitOfWork.Users.Add(user);
            await _unitOfWork.Commit();

            return user.ToUserDTO();
        }

        public async Task<UserDTO> UpdateUser(long userId, UserUpdateDTO userUpdateDTO)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new UserNotFoundException();

            user.FirstName = userUpdateDTO.FirstName;
            user.LastName = userUpdateDTO.LastName;
            if (userUpdateDTO.NewPassword != null)
            {
                user.Password = _passwordService.HashPassword(userUpdateDTO.NewPassword);
            }

            await _unitOfWork.Commit();

            return user.ToUserDTO();
        }

        public async Task<AccessTokenDTO> SignInUser(LoginPayloadDTO loginPayload)
        {
            var user = await _unitOfWork.Users.GetByEmail(loginPayload.Email);

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
            var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new UserNotFoundException();

            return user.ToUserDTO();
        }
    }
}
