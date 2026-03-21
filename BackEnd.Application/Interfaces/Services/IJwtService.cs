using BackEnd.Domain.Entities.Identity;

namespace BackEnd.Application.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, string role, int? donorId, int? staffId);
        string GenerateRefreshToken(); // ✅ جديد
    }
}