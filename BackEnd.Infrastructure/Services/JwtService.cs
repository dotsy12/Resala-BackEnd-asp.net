using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackEnd.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSittings _jwtSettings;

        public JwtService(JwtSittings jwtSettings) => _jwtSettings = jwtSettings;

        public string GenerateToken(ApplicationUser user, string role, int? donorId, int? staffId)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key)); // ✅ مش null

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.Role, role),
            new("fullName", $"{user.FirstName} {user.LastName}".Trim()),
            new("phoneNumber", user.PhoneNumber ?? ""),
        };

            if (donorId.HasValue)
                claims.Add(new("donorId", donorId.Value.ToString()));
            if (staffId.HasValue)
                claims.Add(new("staffId", staffId.Value.ToString()));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.DurationInHours), // ✅ استخدم DurationInHours
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}