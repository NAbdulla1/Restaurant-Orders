using Microsoft.IdentityModel.Tokens;
using Restaurant_Orders.Extensions;
using Restaurant_Orders.Models;
using Restaurant_Orders.Models.Config;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Restaurant_Orders.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }

    public class JsonWebTokenService : ITokenService
    {
        private readonly JWTConfigData _jwtInfo;

        public JsonWebTokenService(IConfiguration configuration)
        {
            _jwtInfo = configuration.GetRequiredSection(JWTConfigData.ConfigSectionName).Get<JWTConfigData>();
        }

        public string CreateToken(User user)
        {
            var jwtClaims = GetJwtClaims();
            var userClaims = GetUserClaims(user);
            var signingCred = GetSigningCredentials();

            var token = new JwtSecurityToken(claims: jwtClaims.Concat(userClaims), signingCredentials: signingCred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private SigningCredentials GetSigningCredentials()
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtInfo.Secret));
            var signingCred = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            return signingCred;
        }

        private List<Claim> GetJwtClaims()
        {
            var nowUtc = DateTime.UtcNow;
            var expiresAtUtc = nowUtc.AddMinutes(_jwtInfo.ExpireInMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _jwtInfo.Subject),
                new Claim(JwtRegisteredClaimNames.Aud, _jwtInfo.Audience),
                new Claim(JwtRegisteredClaimNames.Iss, _jwtInfo.Issuer),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(nowUtc).ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Exp, EpochTime.GetIntDate(expiresAtUtc).ToString(), ClaimValueTypes.Integer64),
            };

            return claims;
        }

        private static List<Claim> GetUserClaims(User user)
        {
            return new List<Claim> {
                new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(user.ToUserDTO())),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType.ToString()),
            };
        }
    }
}
