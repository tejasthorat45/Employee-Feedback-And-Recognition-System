using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FeedbackSystem.API.Entities;
using FeedbackSystem.API.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FeedbackSystem.API.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _opt;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _opt = options.Value;
        }

        public (string token, DateTime expiresAt) CreateToken(User user)
        {
            // Guard: ensure key exists (defensive; Program.cs already validates)
            if (string.IsNullOrWhiteSpace(_opt.Key))
                throw new InvalidOperationException("JWT Key missing in configuration.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),  // Standard: Subject = UserId
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId),  // Custom claim for backend
                new Claim("userId", user.UserId)  // Explicit userId claim for clarity
            };

            // Add role claim only if present to avoid null issues
            var roleName = user.Role?.RoleName;
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(_opt.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expiresAt);
        }
    }
}
