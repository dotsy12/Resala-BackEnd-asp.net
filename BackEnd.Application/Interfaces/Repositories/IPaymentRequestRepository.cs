using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IPaymentRequestRepository
    {
        Task AddAsync(PaymentRequest payment, CancellationToken ct = default);
        Task<PaymentRequest?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<bool> HasPendingPaymentAsync(int subscriptionId, CancellationToken ct = default);
        Task<IReadOnlyList<PaymentRequest>> GetBySubscriptionIdAsync(
            int subscriptionId, CancellationToken ct = default);
        Task<IReadOnlyList<PaymentRequest>> GetAllPendingAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PaymentRequest>> GetPendingByMethodAsync(
            PaymentMethod method, CancellationToken ct = default);
        void Update(PaymentRequest payment);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}