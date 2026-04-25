
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Sponsorship;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class SponsorshipRepository : ISponsorshipRepository
    {
        private readonly ApplicationDbContext _context;

        public SponsorshipRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ للقراءة فقط — لا تستخدم في Update/Delete
        public async Task<Sponsorship?> GetByIdAsync(
            int id, CancellationToken cancellationToken = default)
            => await _context.Sponsorships
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        // ✅ للكتابة (Update/Delete) — بدون AsNoTracking
        public async Task<Sponsorship?> GetByIdTrackedAsync(
            int id, CancellationToken cancellationToken = default)
            => await _context.Sponsorships
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        public async Task<IReadOnlyList<Sponsorship>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => await _context.Sponsorships
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedOn)
                .ToListAsync(cancellationToken);

        public async Task<Sponsorship> CreateAsync(
            Sponsorship sponsorship, CancellationToken cancellationToken = default)
        {
            await _context.Sponsorships.AddAsync(sponsorship, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return sponsorship;
        }

        public async Task UpdateAsync(
            Sponsorship sponsorship, CancellationToken cancellationToken = default)
        {
            // ✅ إذا كانت الـ entity tracked بالفعل (جاءت من GetByIdTrackedAsync)
            // EF Core يتتبع التغييرات تلقائياً — لا نحتاج Update() صريح
            // لكن نستدعيها لضمان الـ tracking في حالة detached
            _context.Sponsorships.Update(sponsorship);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            Sponsorship sponsorship, CancellationToken cancellationToken = default)
        {
            // ✅ Remove يحتاج tracked entity
            // إذا كانت detached (AsNoTracking) سيحدث خطأ
            _context.Sponsorships.Remove(sponsorship);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}