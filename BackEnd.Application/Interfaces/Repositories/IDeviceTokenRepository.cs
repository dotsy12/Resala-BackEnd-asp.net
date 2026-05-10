using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Domain.Entities.Notification;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IDeviceTokenRepository : IGenericRepository<DeviceToken, int>
    {
        Task<List<string>> GetTokensByDonorIdAsync(int donorId, CancellationToken ct = default);
        Task<DeviceToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task RemoveInactiveTokensAsync(DateTime olderThan, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
