using BackEnd.Domain.Entities.Payment;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IInKindDonationRepository
    {
        Task AddAsync(InKindDonation donation, CancellationToken ct = default);
        Task<InKindDonation?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<InKindDonation>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<InKindDonation>> GetByDonorIdAsync(int donorId, CancellationToken ct = default);
        void Update(InKindDonation donation);
        void Remove(InKindDonation donation);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}