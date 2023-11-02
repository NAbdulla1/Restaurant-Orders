using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Restaurant_Orders.Services;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace RestaurantOrder.All.Tests
{
    public class JsonWebTokenServiceShould
    {
        private IConfiguration _configuration;
        private User _userUnderAuth;
        private JWTConfigData _jwtConfig;
        private JsonWebTokenService sut;
        private const int ExpireInMinutes = 1;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var configData = new Dictionary<string, string>
            {
                { "JWTInfo:Subject", "Test Subject" },
                { "JWTInfo:Audience", "Test Env" },
                { "JWTInfo:Issuer", "Restaurant Tester" },
                { "JWTInfo:ExpireInMinutes", $"{ExpireInMinutes}" },
                { "JWTInfo:Secret", "H1ySt7x4DIxMAfesa9cnIg==" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            _jwtConfig = _configuration.GetRequiredSection(JWTConfigData.ConfigSectionName).Get<JWTConfigData>();

            _userUnderAuth = new User
            {
                Id = 1001,
                Email = "example@email.com",
                FirstName = "Test",
                LastName = "Env",
                Password = "1234",
                UserType = UserType.RestaurantOwner
            };
        }

        [SetUp]
        public void SetUp()
        {
            sut = new JsonWebTokenService(_configuration);
        }

        [Test]
        public void ThrowExceptionIfConfigIsNotProvided()
        {
            Assert.Throws<InvalidOperationException>(() => new JsonWebTokenService(new ConfigurationBuilder().AddInMemoryCollection().Build()));
        }

        [Test]
        public void CreateJsonWebToken()
        {
            var token = sut.CreateToken(_userUnderAuth);

            Assert.That(token, Is.Not.Null);
            Assert.That(new JwtSecurityTokenHandler().CanReadToken(token), Is.True);
        }

        [Test]
        public void CreateTokenWithExpiration()
        {
            var token = sut.CreateToken(_userUnderAuth);

            new JwtSecurityTokenHandler().ValidateToken(
                token,
                GetTokenValidationParameters(_jwtConfig.Secret),
                out SecurityToken validatedToken);

            Assert.That(validatedToken.ValidTo, Is.GreaterThan(DateTime.UtcNow));
            Assert.That(validatedToken.ValidTo, Is.LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(ExpireInMinutes)));

            //Thread.Sleep(TimeSpan.FromMinutes(ExpireInMinutes));
            //Assert.That(validatedToken.ValidTo, Is.LessThanOrEqualTo(DateTime.UtcNow));
        }

        [Test]
        public void CreateTokenWithSomeClaims()
        {
            var token = sut.CreateToken(_userUnderAuth);

            var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(
                token,
                GetTokenValidationParameters(_jwtConfig.Secret),
                out SecurityToken validatedToken);

            Assert.That(claimsPrincipal.HasClaim(claim => claim.Type == ClaimTypes.Email), Is.True);
            Assert.That(claimsPrincipal.HasClaim(claim => claim.Type == ClaimTypes.UserData), Is.True);
            Assert.That(claimsPrincipal.HasClaim(claim => claim.Type == ClaimTypes.Role), Is.True);
            Assert.That(claimsPrincipal.HasClaim(claim => claim.Type == JwtRegisteredClaimNames.Exp));
        }

        [Test]
        public void CreateTokenWithUserDataClaim()
        {
            var token = sut.CreateToken(_userUnderAuth);

            var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(
                token,
                GetTokenValidationParameters(_jwtConfig.Secret),
                out SecurityToken _);

            var userDataClaimValue = claimsPrincipal.FindFirstValue(ClaimTypes.UserData);
            var userData = JsonSerializer.Deserialize<UserDTO>(userDataClaimValue);

            Assert.That(userDataClaimValue, Is.Not.Null);
            Assert.That(userData, Is.Not.Null);
            Assert.That(userData.Id, Is.EqualTo(_userUnderAuth.Id));
            Assert.That(userData.FirstName, Is.EqualTo(_userUnderAuth.FirstName));
            Assert.That(userData.LastName, Is.EqualTo(_userUnderAuth.LastName));
            Assert.That(userData.Email, Is.EqualTo(_userUnderAuth.Email));
            Assert.That(userData.UserType, Is.EqualTo(_userUnderAuth.UserType.ToString()));
        }

        [Test]
        public void CreateTokenThatIsValidatableByOtherKey()
        {
            var token = sut.CreateToken(_userUnderAuth);
            var otherSecret = "Xrqlw9lw+JWz0FoR//KQMA==";

            Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
            {
                var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(
                    token,
                    GetTokenValidationParameters(otherSecret),
                    out SecurityToken _);
            });
        }

        private TokenValidationParameters GetTokenValidationParameters(string jwtSecret)
        {
            return new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidAudience = _jwtConfig.Audience,
                ValidateAudience = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidateIssuer = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuerSigningKey = true
            };
        }
    }
}
