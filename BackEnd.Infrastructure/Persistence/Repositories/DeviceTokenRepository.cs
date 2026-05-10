using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Notification;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class DeviceTokenRepository : GenericRepository<DeviceToken, int>, IDeviceTokenRepository
    {
        public DeviceTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<string>> GetTokensByDonorIdAsync(int donorId, CancellationToken ct = default)
        {
            return await _set
                .AsNoTracking()
                .Where(t => t.DonorId == donorId)
                .Select(t => t.Token)
                .ToListAsync(ct);
        }

        public async Task<DeviceToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _set.FirstOrDefaultAsync(t => t.Token == token, ct);
        }

        public async Task RemoveInactiveTokensAsync(DateTime olderThan, CancellationToken ct = default)
        {
            var inactiveTokens = await _set
                .Where(t => t.LastUsed < olderThan)
                .ToListAsync(ct);

            if (inactiveTokens.Any())
            {
                _set.RemoveRange(inactiveTokens);
            }
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
