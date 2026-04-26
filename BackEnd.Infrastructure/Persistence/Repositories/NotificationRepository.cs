using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Notification;
using BackEnd.Infrastructure.Persistence.DbContext;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _db;
        public NotificationRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(Notification notification, CancellationToken ct = default)
            => await _db.Notifications.AddAsync(notification, ct);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
