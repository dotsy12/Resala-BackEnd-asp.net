using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public UserRepository(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public Task<ApplicationUser?> GetByPhoneAsync(string phone, CancellationToken ct)
            => _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, ct);

        public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct)
            => _db.Users.FirstOrDefaultAsync(
                u => u.Email == email.ToLowerInvariant(), ct);

        public Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct)
            => _userManager.FindByNameAsync(username)!;

        public Task<bool> PhoneExistsAsync(string phone, CancellationToken ct)
            => _db.Users.AnyAsync(u => u.PhoneNumber == phone, ct);

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct)
            => _db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

        public async Task<string?> GetRoleAsync(ApplicationUser user, CancellationToken ct)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }
    }
}