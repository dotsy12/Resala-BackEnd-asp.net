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
    public record SubmitBranchSubscriptionPaymentCommand(
        int SubscriptionId,
        int DonorId,
        decimal Amount,
        int SlotId,
        string BranchContactPhone,
        string DonorName
    ) : IRequest<Result<PaymentRequestSummaryDto>>;

    public class SubmitBranchSubscriptionPaymentHandler
        : IRequestHandler<SubmitBranchSubscriptionPaymentCommand, Result<PaymentRequestSummaryDto>>
    {
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly IAppointmentSlotRepository _slotRepo;
        private readonly ILogger<SubmitBranchSubscriptionPaymentHandler> _logger;

        public SubmitBranchSubscriptionPaymentHandler(
            ISponsorshipSubscriptionRepository subRepo,
            IPaymentRequestRepository paymentRepo,
            IAppointmentSlotRepository slotRepo,
            ILogger<SubmitBranchSubscriptionPaymentHandler> logger)
        {
            _subRepo = subRepo;
            _paymentRepo = paymentRepo;
            _slotRepo = slotRepo;
            _logger = logger;
        }

        public async Task<Result<PaymentRequestSummaryDto>> Handle(
            SubmitBranchSubscriptionPaymentCommand request, CancellationToken ct)
        {
            var subscription = await _subRepo.GetByIdAsync(request.SubscriptionId, ct);
            if (subscription is null)
                return Result<PaymentRequestSummaryDto>.Failure("الاشتراك غير موجود.", ErrorType.NotFound);

            if (subscription.DonorId != request.DonorId)
                return Result<PaymentRequestSummaryDto>.Failure("غير مصرّح.", ErrorType.Forbidden);

            var hasPending = await _paymentRepo.HasPendingPaymentAsync(request.SubscriptionId, ct);
            if (hasPending)
                return Result<PaymentRequestSummaryDto>.Failure("لديك طلب دفع معلق بالفعل.", ErrorType.Conflict);

            var slot = await _slotRepo.GetByIdAsync(request.SlotId, ct);
            if (slot is null)
                return Result<PaymentRequestSummaryDto>.Failure("الموعد غير موجود.", ErrorType.NotFound);

            if (!slot.HasAvailableCapacity)
                return Result<PaymentRequestSummaryDto>.Failure("هذا الموعد ممتلئ.", ErrorType.Conflict);

            slot.Book();
            _slotRepo.Update(slot);

            var branchDetails = new BranchPaymentDetails(
                donorName: request.DonorName,
                contactNumber: request.BranchContactPhone,
                scheduledDate: slot.SlotDate.Add(slot.OpenFrom),
                slotId: slot.Id
            );

            var paymentRequest = PaymentRequest.CreateBranch(
                donorId: request.DonorId,
                subscriptionId: subscription.Id,
                emergencyCaseId: null,
                generalDonationId: null,
                amount: new Money(request.Amount),
                branchDetails: branchDetails
            );

            await _paymentRepo.AddAsync(paymentRequest, ct);
            await _paymentRepo.SaveChangesAsync(ct);

            return Result<PaymentRequestSummaryDto>.Success(
                MapToDto(paymentRequest), "تم حجز موعد الدفع في الفرع بنجاح.");
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
            ContactName: null,
            ContactPhone: p.BranchDetails?.ContactNumber,
            Address: null,
            RepresentativeNotes: null,
            DeliveryAreaId: null,
            DeliveryAreaName: null,
            ScheduledDate: p.BranchDetails?.ScheduledDate,
            RejectionReason: null,
            CreatedOn: p.CreatedOn
        );
    }
}