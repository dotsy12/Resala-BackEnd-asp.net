using BackEnd.Domain.Entities.Notification;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IDeliveryAreaRepository
    {
        Task<DeliveryArea?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<DeliveryArea>> GetAllActiveAsync(CancellationToken ct = default);
    }
}