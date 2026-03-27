using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IStaffRepository
    {
        Task AddAsync(StaffMember staff, CancellationToken ct = default);
        Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct = default);
        // IStaffRepository.cs — أضف السطر ده
        Task<StaffMember?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<AccountStatus?> GetStatusByIdAsync(int staffId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}