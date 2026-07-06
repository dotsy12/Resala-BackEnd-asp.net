using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class SupportMessageRepository : ISupportMessageRepository
    {
        private readonly ApplicationDbContext _db;

        public SupportMessageRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<SupportMessage?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.SupportMessages.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<(IReadOnlyList<SupportMessage> Items, int TotalCount)> GetPagedMessagesAsync(
            string chatOwnerUserId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _db.SupportMessages
                .Where(x => x.ChatOwnerUserId == chatOwnerUserId)
                .AsQueryable();

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(x => x.CreatedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public Task<List<SupportMessage>> GetUnreadMessagesAsync(string chatOwnerUserId, CancellationToken ct = default)
        {
            return _db.SupportMessages
                .Where(x => x.ChatOwnerUserId == chatOwnerUserId && !x.IsRead)
                .ToListAsync(ct);
        }

        public async Task AddAsync(SupportMessage message, CancellationToken ct = default)
            => await _db.SupportMessages.AddAsync(message, ct);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
