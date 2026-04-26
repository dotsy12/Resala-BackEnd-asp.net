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
            var payments = await _repo.GetAllPendingAsync(BackEnd.Domain.Enums.PaymentTargetType.Subscription, ct);
            return Result<IReadOnlyList<PaymentRequestSummaryDto>>
                .Success(payments.Select(MapToDto).ToList().AsReadOnly());
        }

        internal static PaymentRequestSummaryDto MapToDto(
            Domain.Entities.Payment.PaymentRequest p)
        {
            var user = p.Donor?.User ?? p.Subscription?.Donor?.User;
            
            return new(
                Id: p.Id,
                SubscriptionId: p.SubscriptionId,
                EmergencyCaseId: p.EmergencyCaseId,
                EmergencyCaseTitle: p.EmergencyCase?.Title,
                UserName: user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
                Phone: user?.PhoneNumber,
                Method: p.Method.ToString(),
                Status: p.Status.ToString(),
                Amount: p.Amount.Amount,
                ReceiptImageUrl: p.ReceiptImageUrl,
                ReceiptImagePublicId: p.ReceiptImagePublicId,
                SenderPhoneNumber: p.SenderPhoneNumber,
                ContactName: p.RepresentativeInfo?.ContactName,
                ContactPhone: p.RepresentativeInfo?.ContactPhone ?? p.BranchDetails?.ContactNumber,
                Address: p.RepresentativeInfo?.Address,
                RepresentativeNotes: p.RepresentativeInfo?.Notes,
                DeliveryAreaId: p.RepresentativeInfo?.DeliveryAreaId,
                DeliveryAreaName: p.RepresentativeInfo?.DeliveryAreaName,
                ScheduledDate: p.BranchDetails?.ScheduledDate,
                RejectionReason: p.RejectionReason,
                CreatedOn: p.CreatedOn
            );
        }
    }
}
