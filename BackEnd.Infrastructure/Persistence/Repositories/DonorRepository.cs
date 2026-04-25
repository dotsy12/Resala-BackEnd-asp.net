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

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
