using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Sponsorship;
using BackEnd.Domain.Enums;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class SponsorshipSubscriptionRepository : ISponsorshipSubscriptionRepository
    {
        private readonly ApplicationDbContext _db;
        public SponsorshipSubscriptionRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(SponsorshipSubscription sub, CancellationToken ct = default)
            => await _db.SponsorshipSubscriptions.AddAsync(sub, ct);

        public Task<SponsorshipSubscription?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.SponsorshipSubscriptions
                .Include(s => s.Donor)
                .Include(s => s.Sponsorship)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

        public Task<SponsorshipSubscription?> GetActiveByDonorAndSponsorshipAsync(
            int donorId, int sponsorshipId, CancellationToken ct = default)
            => _db.SponsorshipSubscriptions
                .FirstOrDefaultAsync(s =>
                    s.DonorId == donorId &&
                    s.SponsorshipId == sponsorshipId &&
                    s.Status == SubscriptionStatus.Active, ct);

        public async Task<IReadOnlyList<SponsorshipSubscription>> GetByDonorIdAsync(
            int donorId, CancellationToken ct = default)
            => await _db.SponsorshipSubscriptions
                .Include(s => s.Sponsorship)
                .Where(s => s.DonorId == donorId)
                .OrderByDescending(s => s.CreatedOn)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<SponsorshipSubscription>> GetAllAsync(
            CancellationToken ct = default)
            => await _db.SponsorshipSubscriptions
                .Include(s => s.Donor)
                .Include(s => s.Sponsorship)
                .OrderByDescending(s => s.CreatedOn)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<SponsorshipSubscription>> GetOverdueAsync(
            int gracePeriodDays, CancellationToken ct = default)
        {
            var threshold = DateTime.UtcNow.AddDays(-gracePeriodDays);
            return await _db.SponsorshipSubscriptions
                .Include(s => s.Donor)
                .Include(s => s.Sponsorship)
                .Where(s =>
                    s.Status == SubscriptionStatus.Active &&
                    s.NextPaymentDate < threshold)
                .ToListAsync(ct);
        }

        public void Update(SponsorshipSubscription sub) => _db.SponsorshipSubscriptions.Update(sub);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}