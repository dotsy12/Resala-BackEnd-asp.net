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
                .Where(a => a.IsActive && !a.IsDeleted)
                .OrderBy(a => a.Governorate)
                .ThenBy(a => a.City)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<DeliveryArea>> GetAllAsync(CancellationToken ct = default)
             => await _db.DeliveryAreas
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Governorate)
                .ThenBy(a => a.City)
                .ToListAsync(ct);

        public async Task AddAsync(DeliveryArea area, CancellationToken ct = default)
            => await _db.DeliveryAreas.AddAsync(area, ct);

        public void Update(DeliveryArea area)
            => _db.DeliveryAreas.Update(area);

        public async Task<bool> IsDuplicateAsync(string name, string governorate, string city, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _db.DeliveryAreas.AsQueryable();
            
            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);

            return await query.AnyAsync(a => 
                a.Name == name && 
                a.Governorate == governorate && 
                a.City == city &&
                !a.IsDeleted, ct);
        }

        public void Delete(DeliveryArea area)=>
            _db.DeliveryAreas.Remove(area);
        
    }
}
