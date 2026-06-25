using BackEnd.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface ICartRepository
    {
        Task AddAsync(CartItem item, CancellationToken ct = default);
        Task<CartItem?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<CartItem?> GetByDonorAndTargetAsync(int donorId, int? sponsorshipId, int? emergencyCaseId, CancellationToken ct = default);
        Task<IReadOnlyList<CartItem>> GetByDonorIdAsync(int donorId, CancellationToken ct = default);
        void Update(CartItem item);
        void Remove(CartItem item);
        void RemoveRange(IEnumerable<CartItem> items);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
