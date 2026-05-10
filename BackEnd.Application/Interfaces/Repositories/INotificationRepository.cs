using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Domain.Entities.Notification;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification, int>
    {
        Task<List<Notification>> GetByDonorIdAsync(int donorId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
