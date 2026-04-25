using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly ApplicationDbContext _db;
        public DonorRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(Donor donor, CancellationToken ct = default)
            => await _db.Donors.AddAsync(donor, ct);

        public async Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct = default)
        {
            var donor = await _db.Donors.FirstOrDefaultAsync(d => d.UserId == userId, ct);
            return donor?.Id;
        }

        public Task<Donor?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id, ct);

        public Task<Donor?> GetByUserIdAsync(string userId, CancellationToken ct = default)
            => _db.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId, ct);

        public async Task<(IReadOnlyList<Donor> Items, int TotalCount)> GetPagedAsync(
            string? search, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Donors
                .Include(d => d.User)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(d =>
                    EF.Functions.Like(d.FullName.FirstName + " " + d.FullName.LastName, $"%{search}%") ||
                    EF.Functions.Like(d.PhoneNumber.Value, $"%{search}%") ||
                    EF.Functions.Like(d.Email.Value, $"%{search}%"));
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(d => d.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<IReadOnlyList<Donor>> GetDropdownAsync(string? search, int count = 20, CancellationToken ct = default)
        {
            var query = _db.Donors.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(d =>
                    EF.Functions.Like(d.FullName.FirstName + " " + d.FullName.LastName, $"%{search}%") ||
                    EF.Functions.Like(d.PhoneNumber.Value, $"%{search}%"));
            }

            return await query
                .OrderByDescending(d => d.Id)
                .Take(count)
                .ToListAsync(ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
