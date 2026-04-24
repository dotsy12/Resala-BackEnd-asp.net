using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Notification;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class DeliveryAreaRepository : IDeliveryAreaRepository
    {
        private readonly ApplicationDbContext _db;
        public DeliveryAreaRepository(ApplicationDbContext db) => _db = db;

        public Task<DeliveryArea?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.DeliveryAreas.FirstOrDefaultAsync(a => a.Id == id, ct);

        public async Task<IReadOnlyList<DeliveryArea>> GetAllActiveAsync(
            CancellationToken ct = default)
            => await _db.DeliveryAreas
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync(ct);
    }
}