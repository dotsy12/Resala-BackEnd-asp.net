using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IPaymentNumberRepository
    {
        Task<PaymentNumber?> GetByTypeAsync(PaymentMethod type, CancellationToken ct = default);
        Task<IReadOnlyList<PaymentNumber>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PaymentNumber>> GetActiveAsync(CancellationToken ct = default);
        Task AddAsync(PaymentNumber paymentNumber, CancellationToken ct = default);
        void Update(PaymentNumber paymentNumber);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
