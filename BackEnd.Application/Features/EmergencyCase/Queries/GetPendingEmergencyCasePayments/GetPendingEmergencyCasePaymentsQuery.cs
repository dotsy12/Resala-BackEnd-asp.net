using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.EmergencyCase.Queries.GetPendingEmergencyCasePayments
{
    public record GetPendingEmergencyCasePaymentsQuery(
        PaymentMethod? Method = null
    ) : IRequest<Result<IReadOnlyList<PaymentRequestSummaryDto>>>;

    public class GetPendingEmergencyCasePaymentsHandler
        : IRequestHandler<GetPendingEmergencyCasePaymentsQuery, Result<IReadOnlyList<PaymentRequestSummaryDto>>>
    {
        private readonly IPaymentRequestRepository _paymentRepo;

        public GetPendingEmergencyCasePaymentsHandler(IPaymentRequestRepository paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        public async Task<Result<IReadOnlyList<PaymentRequestSummaryDto>>> Handle(
            GetPendingEmergencyCasePaymentsQuery request, CancellationToken ct)
        {
            IReadOnlyList<BackEnd.Domain.Entities.Payment.PaymentRequest> payments;

            if (request.Method.HasValue)
            {
                payments = await _paymentRepo.GetPendingByMethodAsync(
                    request.Method.Value, PaymentTargetType.EmergencyCase, ct);
            }
            else
            {
                payments = await _paymentRepo.GetAllPendingAsync(
                    PaymentTargetType.EmergencyCase, ct);
            }

            var result = payments.Select(p => new PaymentRequestSummaryDto(
                Id: p.Id,
                SubscriptionId: p.SubscriptionId,
                EmergencyCaseId: p.EmergencyCaseId,
                EmergencyCaseTitle: p.EmergencyCase?.Title,
                UserName: $"{p.Donor?.FullName.FirstName} {p.Donor?.FullName.LastName}",
                Phone: p.Donor?.PhoneNumber.Value,
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
            )).ToList().AsReadOnly();

            return Result<IReadOnlyList<PaymentRequestSummaryDto>>.Success(result);
        }
    }
}
