using BackEnd.Domain.Entities;
using BackEnd.Domain.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface ISuccessStoryRepository
    {
        Task<SuccessStory?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<SuccessStory>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(SuccessStory story, CancellationToken ct = default);
        void Delete(SuccessStory story);
        Task SaveChangesAsync(CancellationToken ct = default);
    }

    public interface IFeedbackRepository
    {
        Task<Feedback?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<Feedback> Items, int TotalCount)> GetAllPagedAsync(
            FeedbackType? type, 
            FeedbackStatus? status, 
            int pageNumber, 
            int pageSize, 
            CancellationToken ct = default);
        Task AddAsync(Feedback feedback, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
