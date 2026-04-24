using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class PaymentRequestRepository : IPaymentRequestRepository
    {
        private readonly ApplicationDbContext _db;
        public PaymentRequestRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(PaymentRequest payment, CancellationToken ct = default)
            => await _db.PaymentRequests.AddAsync(payment, ct);

        public Task<PaymentRequest?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.PaymentRequests.FirstOrDefaultAsync(p => p.Id == id, ct);

        public Task<bool> HasPendingPaymentAsync(int subscriptionId, CancellationToken ct = default)
            => _db.PaymentRequests.AnyAsync(p =>
                p.SubscriptionId == subscriptionId &&
                p.Status == PaymentStatus.Pending, ct);

        public async Task<IReadOnlyList<PaymentRequest>> GetBySubscriptionIdAsync(
            int subscriptionId, CancellationToken ct = default)
            => await _db.PaymentRequests
                .Where(p => p.SubscriptionId == subscriptionId)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<PaymentRequest>> GetAllPendingAsync(
            CancellationToken ct = default)
            => await _db.PaymentRequests
                .Where(p => p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.CreatedOn)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<PaymentRequest>> GetPendingByMethodAsync(
            PaymentMethod method, CancellationToken ct = default)
            => await _db.PaymentRequests
                .Where(p => p.Method == method && p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.CreatedOn)
                .ToListAsync(ct);

        public void Update(PaymentRequest payment) => _db.PaymentRequests.Update(payment);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}