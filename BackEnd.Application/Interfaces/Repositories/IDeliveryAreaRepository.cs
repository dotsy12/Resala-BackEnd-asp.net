using BackEnd.Domain.Entities.Notification;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IDeliveryAreaRepository
    {
        Task<DeliveryArea?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<DeliveryArea>> GetAllActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<DeliveryArea>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(DeliveryArea area, CancellationToken ct = default);
        void Update(DeliveryArea area);
        void Delete(DeliveryArea area);
        Task<bool> IsDuplicateAsync(string name, string governorate, string city, int? excludeId = null, CancellationToken ct = default);
    }
}
