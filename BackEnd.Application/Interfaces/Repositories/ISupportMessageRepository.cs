using BackEnd.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface ISupportMessageRepository
    {
        Task<SupportMessage?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<SupportMessage> Items, int TotalCount)> GetPagedMessagesAsync(
            string chatOwnerUserId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
        Task<List<SupportMessage>> GetUnreadMessagesAsync(string chatOwnerUserId, CancellationToken ct = default);
        Task AddAsync(SupportMessage message, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
