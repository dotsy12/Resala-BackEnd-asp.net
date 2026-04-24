// GetPendingPaymentsQuery.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetPendingPayments
{
    public class GetPendingPaymentsHandler
        : IRequestHandler<GetPendingPaymentsQuery, Result<IReadOnlyList<PaymentRequestSummaryDto>>>
    {
        private readonly IPaymentRequestRepository _repo;
        public GetPendingPaymentsHandler(IPaymentRequestRepository repo) => _repo = repo;

        public async Task<Result<IReadOnlyList<PaymentRequestSummaryDto>>> Handle(
            GetPendingPaymentsQuery request, CancellationToken ct)
        {
            var payments = await _repo.GetAllPendingAsync(ct);
            return Result<IReadOnlyList<PaymentRequestSummaryDto>>
                .Success(payments.Select(MapToDto).ToList().AsReadOnly());
        }

        internal static PaymentRequestSummaryDto MapToDto(
            Domain.Entities.Payment.PaymentRequest p) => new(
            Id: p.Id,
            Method: p.Method.ToString(),
            Status: p.Status.ToString(),
            Amount: p.Amount.Amount,
            ReceiptImageUrl: p.ReceiptImageUrl,
            ReceiptImagePublicId: p.ReceiptImagePublicId,
            SenderPhoneNumber: p.SenderPhoneNumber,
            ContactName: p.RepresentativeInfo?.ContactName,
            ContactPhone: p.RepresentativeInfo?.ContactPhone ?? p.BranchDetails?.ContactNumber,
            ScheduledDate: p.BranchDetails?.ScheduledDate,
            RejectionReason: p.RejectionReason,
            CreatedOn: p.CreatedOn
        );
    }

   
}