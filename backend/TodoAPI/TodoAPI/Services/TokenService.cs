using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoAPI.Abstractions;
using TodoAPI.Models;

namespace TodoAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenModel GenerateToken(User user)
        {
            var durationInMinutes = _configuration.GetValue<int>("Jwt:DurationInMinutes");
            var expiresAt = DateTime.UtcNow.AddMinutes(durationInMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, user.Role),
                new(JwtRegisteredClaimNames.Jti, new Guid().ToString()),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );
            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return new TokenModel
            {
                TokenValue = tokenValue,
                ExpiresAt = expiresAt
            };
        }
    }
}
