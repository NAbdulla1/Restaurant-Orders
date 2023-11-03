using Moq;
using Restaurant_Orders.Services;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;
using System.Security.Cryptography;

namespace RestaurantOrder.All.Tests
{
    [TestFixture]
    public class UserServiceShould
    {
        private Mock<IPasswordService> _mockPasswordService;
        private Mock<ITokenService> mockTokenService;
        private Mock<IUserRepository> _mockUserRepository;
        private UserService _sut;

        [SetUp]
        public void Setup()
        {
            _mockPasswordService = new Mock<IPasswordService>();
            var _passwordService = _mockPasswordService.Object;

            mockTokenService = new Mock<ITokenService>();
            var _tokenService = mockTokenService.Object;

            _mockUserRepository = new Mock<IUserRepository>();
            var _userRepository = _mockUserRepository.Object;

            _sut = new UserService(_userRepository, _passwordService, _tokenService);
        }

        [Test]
        public async Task CreateCustomerIfNotAlreadyExists()
        {
            //Arrange
            var customerRegisterDTO = new CustomerRegisterDTO
            {
                FirstName = "Test",
                LastName = "Customer",
                Email = "a@b.c",
                Password = "password",
                ConfirmPassword = "password"
            };
            var dbGeneratedUserId = 1;
            var hashedPassword = $"hashedPassword: {customerRegisterDTO.Password}";
            _mockPasswordService.Setup(ps => ps.HashPassword(hashedPassword));

            _mockUserRepository.Setup(ur => ur.GetByEmail(It.IsAny<string>())).Returns(() => Task.FromResult<User?>(null));
            _mockUserRepository.Setup(ur => ur.Add(It.IsAny<User>())).Returns(new User
            {
                Id = dbGeneratedUserId,
                FirstName = customerRegisterDTO.FirstName,
                LastName = customerRegisterDTO.LastName,
                Email = customerRegisterDTO.Email,
                Password = hashedPassword,
                UserType = UserType.Customer
            });

            //Act
            var userDTO = await _sut.CreateCustomer(customerRegisterDTO);

            //Assert
            Assert.That(userDTO, Is.Not.Null);
            Assert.That(userDTO.Id, Is.EqualTo(dbGeneratedUserId));
            Assert.That(userDTO.FirstName, Is.EqualTo(customerRegisterDTO.FirstName));
            Assert.That(userDTO.LastName, Is.EqualTo(customerRegisterDTO.LastName));
            Assert.That(userDTO.Email, Is.EqualTo(customerRegisterDTO.Email));
            Assert.That(userDTO.UserType, Is.EqualTo(UserType.Customer.ToString()));
        }

        [Test]
        public void ThrowExceptionIfCustomerAlreadyExists()
        {
            //Arrange
            var customer = new CustomerRegisterDTO
            {
                FirstName = "Test",
                LastName = "Customer",
                Email = "a@b.c",
                Password = "password",
                ConfirmPassword = "password"
            };
            var generatedUserId = 1;
            var hashedPassword = $"hashedPassword: {customer.Password}";
            _mockPasswordService.Setup(ps => ps.HashPassword(hashedPassword));

            _mockUserRepository.Setup(ur => ur.GetByEmail(customer.Email)).ReturnsAsync(new User
            {
                Id = generatedUserId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Password = hashedPassword,
                UserType = UserType.Customer
            });

            //Act & Assert
            var exception = Assert.ThrowsAsync<CustomerAlreadyExistsException>(async () => await _sut.CreateCustomer(customer));
            Assert.That(exception.Message, Is.EqualTo($"Another user already exists with the given email: '{customer.Email}'."));
        }
    }
}
