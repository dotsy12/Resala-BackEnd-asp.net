using BackEnd.Domain.Entities.Sponsorship;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface ISponsorshipSubscriptionRepository
    {
        Task AddAsync(SponsorshipSubscription subscription, CancellationToken ct = default);
        Task<SponsorshipSubscription?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<SponsorshipSubscription?> GetActiveByDonorAndSponsorshipAsync(
            int donorId, int sponsorshipId, CancellationToken ct = default);
        Task<IReadOnlyList<SponsorshipSubscription>> GetByDonorIdAsync(
            int donorId, CancellationToken ct = default);
        Task<IReadOnlyList<SponsorshipSubscription>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<SponsorshipSubscription>> GetOverdueAsync(
            int gracePeriodDays, CancellationToken ct = default);
        void Update(SponsorshipSubscription subscription);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}