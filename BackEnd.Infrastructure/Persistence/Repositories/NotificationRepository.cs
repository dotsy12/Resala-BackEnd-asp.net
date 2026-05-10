using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Notification;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : GenericRepository<Notification, int>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetByDonorIdAsync(int donorId, CancellationToken ct = default)
        {
            return await _set
                .Where(n => n.DonorId == donorId)
                .OrderByDescending(n => n.CreatedOn)
                .ToListAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
