using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetPendingPayments
{
    public class GetPendingPaymentsByMethodHandler
        : IRequestHandler<GetPendingPaymentsByMethodQuery, Result<IReadOnlyList<PaymentRequestSummaryDto>>>
    {
        private readonly IPaymentRequestRepository _repo;
        public GetPendingPaymentsByMethodHandler(IPaymentRequestRepository repo) => _repo = repo;

        public async Task<Result<IReadOnlyList<PaymentRequestSummaryDto>>> Handle(
            GetPendingPaymentsByMethodQuery request, CancellationToken ct)
        {
            var payments = await _repo.GetPendingByMethodAsync(request.Method, ct);
            return Result<IReadOnlyList<PaymentRequestSummaryDto>>
                .Success(payments.Select(GetPendingPaymentsHandler.MapToDto).ToList().AsReadOnly());
        }
    }
}
