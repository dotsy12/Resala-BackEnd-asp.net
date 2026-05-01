using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.SubmitPayment
{
    public record SubmitRepresentativeSubscriptionPaymentCommand(
        int SubscriptionId,
        int DonorId,
        decimal Amount,
        int DeliveryAreaId,
        string Address,
        string ContactName,
        string ContactPhone,
        string? RepresentativeNotes
    ) : IRequest<Result<PaymentRequestSummaryDto>>;

    public class SubmitRepresentativeSubscriptionPaymentHandler
        : IRequestHandler<SubmitRepresentativeSubscriptionPaymentCommand, Result<PaymentRequestSummaryDto>>
    {
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly IDeliveryAreaRepository _areaRepo;
        private readonly ILogger<SubmitRepresentativeSubscriptionPaymentHandler> _logger;

        public SubmitRepresentativeSubscriptionPaymentHandler(
            ISponsorshipSubscriptionRepository subRepo,
            IPaymentRequestRepository paymentRepo,
            IDeliveryAreaRepository areaRepo,
            ILogger<SubmitRepresentativeSubscriptionPaymentHandler> logger)
        {
            _subRepo = subRepo;
            _paymentRepo = paymentRepo;
            _areaRepo = areaRepo;
            _logger = logger;
        }

        public async Task<Result<PaymentRequestSummaryDto>> Handle(
            SubmitRepresentativeSubscriptionPaymentCommand request, CancellationToken ct)
        {
            var subscription = await _subRepo.GetByIdAsync(request.SubscriptionId, ct);
            if (subscription is null)
                return Result<PaymentRequestSummaryDto>.Failure("الاشتراك غير موجود.", ErrorType.NotFound);

            if (subscription.DonorId != request.DonorId)
                return Result<PaymentRequestSummaryDto>.Failure("غير مصرّح.", ErrorType.Forbidden);

            var hasPending = await _paymentRepo.HasPendingPaymentAsync(request.SubscriptionId, ct);
            if (hasPending)
                return Result<PaymentRequestSummaryDto>.Failure("لديك طلب دفع معلق بالفعل.", ErrorType.Conflict);

            var area = await _areaRepo.GetByIdAsync(request.DeliveryAreaId, ct);
            if (area is null)
                return Result<PaymentRequestSummaryDto>.Failure("منطقة التوصيل غير موجودة.", ErrorType.NotFound);

            var repDetails = new RepresentativeDetails(
                deliveryAreaId: area.Id,
                deliveryAreaName: area.Name,
                contactName: request.ContactName,
                contactPhone: request.ContactPhone,
                address: request.Address,
                notes: request.RepresentativeNotes
            );

            var paymentRequest = PaymentRequest.CreateRepresentative(
                donorId: request.DonorId,
                subscriptionId: subscription.Id,
                emergencyCaseId: null,
                generalDonationId: null,
                amount: new Money(request.Amount),
                repDetails: repDetails
            );

            await _paymentRepo.AddAsync(paymentRequest, ct);
            await _paymentRepo.SaveChangesAsync(ct);

            return Result<PaymentRequestSummaryDto>.Success(
                MapToDto(paymentRequest), "تم استلام طلب مندوب التحصيل بنجاح.");
        }

        private static PaymentRequestSummaryDto MapToDto(PaymentRequest p) => new(
            Id: p.Id,
            SubscriptionId: p.SubscriptionId,
            EmergencyCaseId: p.EmergencyCaseId,
            EmergencyCaseTitle: p.EmergencyCase?.Title,
            UserName: null, 
            Phone: null,
            Method: p.Method.ToString(),
            Status: p.Status.ToString(),
            Amount: p.Amount.Amount,
            ReceiptImageUrl: null,
            ReceiptImagePublicId: null,
            SenderPhoneNumber: null,
            ContactName: p.RepresentativeInfo?.ContactName,
            ContactPhone: p.RepresentativeInfo?.ContactPhone,
            Address: p.RepresentativeInfo?.Address,
            RepresentativeNotes: p.RepresentativeInfo?.Notes,
            DeliveryAreaId: p.RepresentativeInfo?.DeliveryAreaId,
            DeliveryAreaName: p.RepresentativeInfo?.DeliveryAreaName,
            ScheduledDate: null,
            RejectionReason: null,
            CreatedOn: p.CreatedOn
        );
    }
}