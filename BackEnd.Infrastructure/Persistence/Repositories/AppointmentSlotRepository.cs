using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class AppointmentSlotRepository : IAppointmentSlotRepository
    {
        private readonly ApplicationDbContext _db;
        public AppointmentSlotRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(BranchAppointmentSlot slot, CancellationToken ct = default)
            => await _db.BranchAppointmentSlots.AddAsync(slot, ct);

        public Task<BranchAppointmentSlot?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.BranchAppointmentSlots.FirstOrDefaultAsync(s => s.Id == id, ct);

        public async Task<IReadOnlyList<BranchAppointmentSlot>> GetAvailableAsync(
            CancellationToken ct = default)
            => await _db.BranchAppointmentSlots
                .Where(s => s.IsActive &&
                            s.SlotDate >= DateTime.UtcNow.Date &&
                            s.BookedCount < s.MaxCapacity)
                .OrderBy(s => s.SlotDate)
                .ThenBy(s => s.OpenFrom)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<BranchAppointmentSlot>> GetAllAsync(
            CancellationToken ct = default)
            => await _db.BranchAppointmentSlots
                .OrderByDescending(s => s.SlotDate)
                .ToListAsync(ct);

        public void Update(BranchAppointmentSlot slot) => _db.BranchAppointmentSlots.Update(slot);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}