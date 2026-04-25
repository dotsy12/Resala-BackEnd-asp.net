using BackEnd.Domain.Entities.Sponsorship;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface ISponsorshipRepository
    {
        // للقراءة فقط (GET endpoints)
        Task<Sponsorship?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Sponsorship>> GetAllAsync(CancellationToken cancellationToken = default);

        // ✅ جديد — للعمليات الكتابية (Update/Delete)
        Task<Sponsorship?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

        Task<Sponsorship> CreateAsync(Sponsorship sponsorship, CancellationToken cancellationToken = default);
        Task UpdateAsync(Sponsorship sponsorship, CancellationToken cancellationToken = default);
        Task DeleteAsync(Sponsorship sponsorship, CancellationToken cancellationToken = default);
    }
}
