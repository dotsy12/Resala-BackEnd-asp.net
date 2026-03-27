using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly ApplicationDbContext _db;
        public StaffRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(StaffMember staff, CancellationToken ct)
            => await _db.StaffMembers.AddAsync(staff, ct);

        public Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct)
            => _db.StaffMembers
                .Where(s => s.UserId == userId)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync(ct);

        public Task<AccountStatus?> GetStatusByIdAsync(int staffId, CancellationToken ct)
            => _db.StaffMembers
                .Where(s => s.Id == staffId)
                .Select(s => (AccountStatus?)s.AccountStatus)
                .FirstOrDefaultAsync(ct);
        // StaffRepository.cs
        public Task<StaffMember?> GetByIdAsync(int id, CancellationToken ct)
            => _db.StaffMembers
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        public Task SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}