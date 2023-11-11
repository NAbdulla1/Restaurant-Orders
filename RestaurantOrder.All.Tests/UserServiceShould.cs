using Moq;
using Restaurant_Orders.Services;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Data;
using RestaurantOrder.Data.Models;

namespace RestaurantOrder.All.Tests
{
    [TestFixture]
    public class UserServiceShould
    {
        private Mock<IPasswordService> _mockPasswordService;
        private Mock<ITokenService> _mockTokenService;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private UserService _sut;

        [SetUp]
        public void Setup()
        {
            _mockPasswordService = new Mock<IPasswordService>();
            var _passwordService = _mockPasswordService.Object;

            _mockTokenService = new Mock<ITokenService>();
            var _tokenService = _mockTokenService.Object;

            _mockUnitOfWork = new Mock<IUnitOfWork>();
            var _unitOfWork = _mockUnitOfWork.Object;

            _sut = new UserService(_unitOfWork, _passwordService, _tokenService);
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

            var hashedPassword = $"hashedPassword: {customerRegisterDTO.Password}";
            _mockPasswordService.Setup(ps => ps.HashPassword(hashedPassword));

            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByEmail(It.IsAny<string>())).Returns(() => Task.FromResult<User?>(null));
            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Commit()).ReturnsAsync(1);

            //Act
            var userDTO = await _sut.CreateCustomer(customerRegisterDTO);

            //Assert
            Assert.That(userDTO, Is.Not.Null);
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

            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByEmail(customer.Email))
                .ReturnsAsync(new User
                {
                    Id = generatedUserId,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Password = hashedPassword,
                    UserType = UserType.Customer
                });

            //Act & Assert
            Assert.That(async () => await _sut.CreateCustomer(customer), Throws.TypeOf<CustomerAlreadyExistsException>());
            Assert.That(async () => await _sut.CreateCustomer(customer), Throws.TypeOf<CustomerAlreadyExistsException>()
                .With
                .Property("Message")
                .EqualTo($"Another user already exists with the given email: '{customer.Email}'."));
        }
    }
}
