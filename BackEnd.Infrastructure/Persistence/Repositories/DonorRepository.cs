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

        public async Task AddAsync(Donor donor, CancellationToken ct)
            => await _db.Donors.AddAsync(donor, ct);

        public Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct)
            => _db.Donors
                .Where(d => d.UserId == userId)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync(ct);
       
        public Task<Donor?> GetByIdAsync(int id, CancellationToken ct)
            => _db.Donors
                .FirstOrDefaultAsync(d => d.Id == id, ct);
        public Task SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}