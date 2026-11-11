    using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TskMgmt.Api.Entities;
using TskMgmt.Api.Interface;

namespace TskMgmt.Api.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration; 
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(
                    jwtSettings["SecretKey"]!)
                );

            var creds = new SigningCredentials(
                key, 
                SecurityAlgorithms.HmacSha256
                );

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expirationMinutes = DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["ExpirationMinutes"]!)
                );

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expirationMinutes,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public ClaimsPrincipal ValidateToken(string accessToken)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)
            );

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],

                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(
                accessToken,
                validationParameters,
                out SecurityToken validatedToken
            );

            return principal;
        }

    }
}
