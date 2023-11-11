using Microsoft.AspNetCore.Http;
using Moq;
using Restaurant_Orders.Services;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Data;
using RestaurantOrder.Data.Models;
using System.Security.Claims;
using System.Text.Json;

namespace RestaurantOrder.All.Tests
{
    [TestFixture]
    public class UserServiceTests
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
        public async Task CreateCustomer_ShouldCreateNewCustomer_WhenCustomerNotAlreadyExists()
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
        public void CreateCustomer_ShouldThrowException_IfCustomerAlreadyExists()
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

        [Test]
        public void UpdateUser_ShouldThrowException_WhenCustomerNotExists()
        {
            //Arrange
            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByIdAsync(It.IsAny<long>())).Returns(Task.FromResult<User?>(null));

            //Act and assert
            Assert.That(
                async () => await _sut.UpdateUser(1, new UserUpdateDTO { FirstName = "Customer", LastName = "Or Admin" }),
                Throws.TypeOf<UserNotFoundException>());
        }

        [TestCase("u fname", "u lname")]
        [TestCase("u fname", "lname")]
        [TestCase("Fname", "upd lname")]
        [TestCase("Fname", "lname")]
        public async Task UpdateUser_ShouldUpdateNamesButNotEmailAndType(string firstName, string lastName)
        {
            //Arrange
            var existingUser = new User
            {
                Id = 1,
                FirstName = "Fname",
                LastName = "lname",
                Email = "a@b.c",
                Password = "pass",
                UserType = UserType.RestaurantOwner
            };

            var updateData = new UserUpdateDTO
            {
                FirstName = firstName,
                LastName = lastName,
            };

            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByIdAsync(existingUser.Id))
                .ReturnsAsync(existingUser);

            //Act
            var updatedUserDTO = await _sut.UpdateUser(existingUser.Id, updateData);

            //Assert
            Assert.That(updatedUserDTO.FirstName, Is.EqualTo(updateData.FirstName));
            Assert.That(updatedUserDTO.LastName, Is.EqualTo(updateData.LastName));
            Assert.That(
                updatedUserDTO.FirstName == existingUser.FirstName,
                Is.EqualTo(updateData.FirstName == existingUser.FirstName));
            Assert.That(
                updatedUserDTO.LastName == existingUser.LastName,
                Is.EqualTo(updateData.LastName == existingUser.LastName));
            Assert.That(updatedUserDTO.Email, Is.EqualTo(existingUser.Email));
            Assert.That(updatedUserDTO.UserType, Is.EqualTo(existingUser.UserType.ToString()));
        }

        [Test]
        public async Task UpdateUser_ShouldUpdatePassword_WhenProvided()
        {
            //Arrange
            var existingUser = new User
            {
                Id = 1,
                FirstName = "Fname",
                LastName = "lname",
                Email = "a@b.c",
                Password = "pass",
                UserType = UserType.RestaurantOwner
            };

            var updateData = new UserUpdateDTO
            {
                NewPassword = "new password",
                ConfirmPasword = "new password"
            };

            var hashPassword = (string pass) => $"hashed {pass}";
            _mockPasswordService.Setup(ps => ps.HashPassword(It.IsAny<string>()))
                .Returns(hashPassword(updateData.NewPassword));

            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByIdAsync(existingUser.Id))
                .ReturnsAsync(existingUser);

            //Act
            var updatedUserDTO = await _sut.UpdateUser(existingUser.Id, updateData);

            //Assert
            _mockUnitOfWork.Verify(_mockUnitOfWork => _mockUnitOfWork.Commit(), Times.Once);
            Assert.That(updatedUserDTO.FirstName, Is.EqualTo(existingUser.FirstName));
            Assert.That(updatedUserDTO.LastName, Is.EqualTo(existingUser.LastName));
            Assert.That(updatedUserDTO.Email, Is.EqualTo(existingUser.Email));
            Assert.That(updatedUserDTO.UserType, Is.EqualTo(existingUser.UserType.ToString()));
        }

        [Test]
        public void SignInUser_ShouldThrowException_WhenUserNotExists()
        {
            //Arrange
            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByEmail(It.IsAny<string>())).Returns(Task.FromResult<User?>(null));
            var loginData = new LoginPayloadDTO
            {
                Email = "a@b.c",
                Password = "password"
            };

            //Act and Assert
            Assert.That(
                async () => await _sut.SignInUser(loginData),
                Throws.TypeOf<UnauthenticatedException>()
                .With
                .Message
                .EqualTo("User authentication failed."));
        }

        [Test]
        public void SignInUser_ShouldThrowException_WhenPasswordIsWrong()
        {
            //Arrange
            var loginData = new LoginPayloadDTO
            {
                Email = "a@b.c",
                Password = "password"
            };
            var existingUser = new User
            {
                Id = 1,
                FirstName = "Fname",
                LastName = "lname",
                Email = "a@b.c",
                Password = "pass",
                UserType = UserType.RestaurantOwner
            };
            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByEmail(loginData.Email))
                .ReturnsAsync(existingUser);

            _mockPasswordService.Setup(ps => ps.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            //Act and Assert
            Assert.That(
                async () => await _sut.SignInUser(loginData),
                Throws.TypeOf<UnauthenticatedException>()
                .With
                .Message
                .EqualTo("User authentication failed."));
        }

        [Test]
        public async Task SignInUser_ShouldReturnAccessToken_WhenUserIsAuthenticated()
        {
            //Arrange
            var loginData = new LoginPayloadDTO
            {
                Email = "a@b.c",
                Password = "password"
            };

            var existingUser = new User
            {
                Id = 1,
                FirstName = "Fname",
                LastName = "lname",
                Email = "a@b.c",
                Password = "pass",
                UserType = UserType.RestaurantOwner
            };

            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByEmail(loginData.Email))
                .ReturnsAsync(existingUser);

            _mockPasswordService.Setup(ps => ps.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _mockTokenService.Setup(ts => ts.CreateToken(existingUser))
                .Returns("jwt token");

            //Act
            var token = await _sut.SignInUser(loginData);

            //Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token.AccessToken, Is.Not.Null);
            Assert.That(token.AccessToken, Has.Length.GreaterThan(0));
        }

        [Test]
        public void GetCurrentAuthenticatedUser_ShouldThrowException_WhenUserIsNotAuthenticated()
        {
            var mockHttpContext = new Mock<HttpContext>();

            Assert.That(
                () => _sut.GetCurrentAuthenticatedUser(mockHttpContext.Object),
                Throws.TypeOf<UnauthenticatedException>());
        }

        [Test]
        public void GetCurrentAuthenticatedUser_ShouldThrowException_WhenUserHasNoClaim()
        {
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(context => context.User)
                .Returns(new System.Security.Claims.ClaimsPrincipal());

            Assert.That(
                () => _sut.GetCurrentAuthenticatedUser(mockHttpContext.Object),
                Throws.TypeOf<UnauthenticatedException>());
        }

        [Test]
        public void GetCurrentAuthenticatedUser_ShouldRetrieveUserFromClaim()
        {
            //Arrange
            var expectedUserDto = new UserDTO
            {
                FirstName = "fname",
                LastName = "lname",
                Email = "a@b.c",
                Id = 1,
                UserType = UserType.Customer.ToString()
            };

            var userDataJson = JsonSerializer.Serialize(expectedUserDto);
            var cp = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.UserData, userDataJson)
                }));
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(context => context.User)
                .Returns(cp);

            //Act
            var actualUserDto = _sut.GetCurrentAuthenticatedUser(mockHttpContext.Object);

            //Assert
            Assert.That(actualUserDto, Is.Not.Null);
            Assert.That(expectedUserDto.Id, Is.EqualTo(actualUserDto.Id));
            Assert.That(expectedUserDto.FirstName, Is.EqualTo(actualUserDto.FirstName));
            Assert.That(expectedUserDto.LastName, Is.EqualTo(actualUserDto.LastName));
            Assert.That(expectedUserDto.Email, Is.EqualTo(actualUserDto.Email));
            Assert.That(expectedUserDto.UserType, Is.EqualTo(actualUserDto.UserType));
        }

        [Test]
        public void GetUser_ShouldThrowException_WhenUserNotExist()
        {
            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByIdAsync(It.IsAny<long>()))
                .Returns(Task.FromResult<User?>(null));

            Assert.That(async () => await _sut.GetUser(1), Throws.TypeOf<UserNotFoundException>());
        }

        [Test]
        public async Task GetUser_ShouldReturnUserDTO_WhenUserExist()
        {
            var existingUser = new User
            {
                Id = 1,
                FirstName = "Fname",
                LastName = "lname",
                Email = "a@b.c",
                Password = "pass",
                UserType = UserType.RestaurantOwner
            };
            _mockUnitOfWork.Setup(unitOfWork => unitOfWork.Users.GetByIdAsync(existingUser.Id))
                .ReturnsAsync(existingUser);

            var userDto = await _sut.GetUser(existingUser.Id);

            Assert.That(userDto, Is.Not.Null);
            Assert.That(userDto.Id, Is.EqualTo(existingUser.Id));
            Assert.That(userDto.FirstName, Is.EqualTo(existingUser.FirstName));
            Assert.That(userDto.LastName, Is.EqualTo(existingUser.LastName));
            Assert.That(userDto.Email, Is.EqualTo(existingUser.Email));
            Assert.That(userDto.UserType, Is.EqualTo(existingUser.UserType.ToString()));
        }
    }
}
