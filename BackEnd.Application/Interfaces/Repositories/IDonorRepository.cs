using BackEnd.Domain.Entities.Identity;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IDonorRepository
    {
        Task AddAsync(Donor donor, CancellationToken ct = default);
        Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct = default);
        // IDonorRepository.cs — أضف السطر ده
        Task<Donor?> GetByIdAsync(int id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}