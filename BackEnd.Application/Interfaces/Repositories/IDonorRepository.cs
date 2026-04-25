using BackEnd.Domain.Entities.Identity;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IDonorRepository
    {
        Task AddAsync(Donor donor, CancellationToken ct = default);
        Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct = default);
        Task<Donor?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Donor?> GetByUserIdAsync(string userId, CancellationToken ct = default); // ✅ Added
        Task<(IReadOnlyList<Donor> Items, int TotalCount)> GetPagedAsync(
            string? search, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<IReadOnlyList<Donor>> GetDropdownAsync(string? search, int count = 20, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
