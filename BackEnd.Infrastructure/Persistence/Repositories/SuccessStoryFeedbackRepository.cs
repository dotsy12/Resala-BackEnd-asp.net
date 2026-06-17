using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities;
using BackEnd.Domain.Enums;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class SuccessStoryRepository : ISuccessStoryRepository
    {
        private readonly ApplicationDbContext _db;
        public SuccessStoryRepository(ApplicationDbContext db) => _db = db;

        public Task<SuccessStory?> GetByIdAsync(int id, CancellationToken ct)
            => _db.SuccessStories.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<IReadOnlyList<SuccessStory>> GetAllAsync(CancellationToken ct)
            => await _db.SuccessStories.OrderByDescending(x => x.CreatedOn).ToListAsync(ct);

        public async Task AddAsync(SuccessStory story, CancellationToken ct)
            => await _db.SuccessStories.AddAsync(story, ct);

        public void Delete(SuccessStory story) => _db.SuccessStories.Remove(story);

        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }

    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly ApplicationDbContext _db;
        public FeedbackRepository(ApplicationDbContext db) => _db = db;

        public Task<Feedback?> GetByIdAsync(int id, CancellationToken ct)
            => _db.Feedbacks.Include(f => f.Donor).FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetAllPagedAsync(
            FeedbackType? type, FeedbackStatus? status, int pageNumber, int pageSize, CancellationToken ct)
        {
            var query = _db.Feedbacks.Include(f => f.Donor).AsQueryable();

            if (type.HasValue) query = query.Where(x => x.Type == type.Value);
            if (status.HasValue) query = query.Where(x => x.Status == status.Value);

            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(x => x.CreatedOn)
                                   .Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(ct);

            return (items, total);
        }

        public async Task AddAsync(Feedback feedback, CancellationToken ct)
            => await _db.Feedbacks.AddAsync(feedback, ct);

        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
