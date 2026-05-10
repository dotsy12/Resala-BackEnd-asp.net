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

        public Task<StaffMember?> GetByIdWithUserAsync(int id, CancellationToken ct)
            => _db.StaffMembers
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

        public async Task<(IReadOnlyList<StaffMember> Items, int TotalCount)> GetStaffWithPaginationAsync(
            string? search, int pageNumber, int pageSize, CancellationToken ct)
        {
            var query = _db.StaffMembers
                .Include(s => s.User)
                .Where(s => !s.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(s => 
                    s.FullName.FirstName.Contains(searchLower) || 
                    s.FullName.LastName.Contains(searchLower) ||
                    s.Email.Value.Contains(searchLower) ||
                    s.Username.Contains(searchLower));
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(s => s.CreatedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public Task SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}