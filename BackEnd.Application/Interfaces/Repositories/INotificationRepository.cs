using BackEnd.Domain.Entities.Notification;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
