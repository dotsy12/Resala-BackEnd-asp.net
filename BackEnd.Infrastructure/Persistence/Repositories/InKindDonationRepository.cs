using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class InKindDonationRepository : IInKindDonationRepository
    {
        private readonly ApplicationDbContext _db;

        public InKindDonationRepository(ApplicationDbContext db)
            => _db = db;

        public async Task AddAsync(InKindDonation donation, CancellationToken ct = default)
            => await _db.InKindDonations.AddAsync(donation, ct);

        public async Task<InKindDonation?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _db.InKindDonations
                .Include(d => d.Donor)
                .Include(d => d.RecordedBy)
                .FirstOrDefaultAsync(d => d.Id == id, ct);

        public async Task<IReadOnlyList<InKindDonation>> GetAllAsync(CancellationToken ct = default)
            => await _db.InKindDonations
                .Include(d => d.Donor)
                .Include(d => d.RecordedBy)
                .OrderByDescending(d => d.RecordedAt)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<InKindDonation>> GetByDonorIdAsync(
            int donorId, CancellationToken ct = default)
            => await _db.InKindDonations
                .Include(d => d.Donor)
                .Include(d => d.RecordedBy)
                .Where(d => d.DonorId == donorId)
                .OrderByDescending(d => d.RecordedAt)
                .ToListAsync(ct);

        public void Update(InKindDonation donation)
            => _db.InKindDonations.Update(donation);

        public void Remove(InKindDonation donation)
            => _db.InKindDonations.Remove(donation);

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}