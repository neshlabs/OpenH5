using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nesh.Repository.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Host.API.Services
{
    public class JWTConfig
    {
        public string SecurityKey { get; set; }
        public int ExpiresMinutes { get; set; }
    }

    public interface IJWTService
    {
        string GetAccessToken(Account account);
    }

    public class JWTService : IJWTService
    {
        private IOptions<JWTConfig> Configuration;

        public JWTService(IOptions<JWTConfig> configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string GetAccessToken(Account account)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Role, account.platform_id.ToString()),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration.Value.SecurityKey));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Configuration.Value.ExpiresMinutes), //Token would be available for one hour
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
