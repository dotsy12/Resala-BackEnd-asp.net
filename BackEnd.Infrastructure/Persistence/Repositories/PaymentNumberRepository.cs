using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class PaymentNumberRepository : IPaymentNumberRepository
    {
        private readonly ApplicationDbContext _db;

        public PaymentNumberRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<PaymentNumber?> GetByTypeAsync(PaymentMethod type, CancellationToken ct)
            => _db.PaymentNumbers.FirstOrDefaultAsync(x => x.Type == type, ct);

        public async Task<IReadOnlyList<PaymentNumber>> GetAllAsync(CancellationToken ct)
            => await _db.PaymentNumbers.ToListAsync(ct);

        public async Task<IReadOnlyList<PaymentNumber>> GetActiveAsync(CancellationToken ct)
            => await _db.PaymentNumbers.Where(x => x.IsActive).ToListAsync(ct);

        public async Task AddAsync(PaymentNumber paymentNumber, CancellationToken ct)
            => await _db.PaymentNumbers.AddAsync(paymentNumber, ct);

        public void Update(PaymentNumber paymentNumber)
            => _db.PaymentNumbers.Update(paymentNumber);

        public Task SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
