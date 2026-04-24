using BackEnd.Domain.Entities.Payment;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IAppointmentSlotRepository
    {
        Task AddAsync(BranchAppointmentSlot slot, CancellationToken ct = default);
        Task<BranchAppointmentSlot?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<BranchAppointmentSlot>> GetAvailableAsync(CancellationToken ct = default);
        Task<IReadOnlyList<BranchAppointmentSlot>> GetAllAsync(CancellationToken ct = default);
        void Update(BranchAppointmentSlot slot);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}