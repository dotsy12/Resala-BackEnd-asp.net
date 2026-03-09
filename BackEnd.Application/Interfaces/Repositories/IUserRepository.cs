using BackEnd.Domain.Entities.Identity;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByPhoneAsync(string phone, CancellationToken ct = default);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task<bool> PhoneExistsAsync(string phone, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
        Task<string?> GetRoleAsync(ApplicationUser user, CancellationToken ct = default);
    }
}