// SubmitPaymentHandler.cs
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Common.Files;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.SubmitPayment
{
    public class SubmitPaymentHandler
        : IRequestHandler<SubmitPaymentCommand, Result<PaymentRequestSummaryDto>>
    {
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly IDeliveryAreaRepository _deliveryAreaRepo;
        private readonly IAppointmentSlotRepository _slotRepo;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<SubmitPaymentHandler> _logger;

        public SubmitPaymentHandler(
            ISponsorshipSubscriptionRepository subRepo,
            IPaymentRequestRepository paymentRepo,
            IDeliveryAreaRepository deliveryAreaRepo,
            IAppointmentSlotRepository slotRepo,
            IFileUploadService fileUploadService,
            ILogger<SubmitPaymentHandler> logger)
        {
            _subRepo = subRepo;
            _paymentRepo = paymentRepo;
            _deliveryAreaRepo = deliveryAreaRepo;
            _slotRepo = slotRepo;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Result<PaymentRequestSummaryDto>> Handle(
            SubmitPaymentCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            _logger.LogInformation(
                "تقديم دفعة — SubscriptionId:{SubId} Method:{Method} DonorId:{DonorId}",
                request.SubscriptionId, dto.PaymentMethod, request.DonorId);

            // التحقق من الاشتراك
            var subscription = await _subRepo.GetByIdAsync(request.SubscriptionId, ct);
            if (subscription is null)
                return Result<PaymentRequestSummaryDto>.Failure(
                    "الاشتراك غير موجود.", ErrorType.NotFound);

            // التحقق من الملكية
            if (subscription.DonorId != request.DonorId)
                return Result<PaymentRequestSummaryDto>.Failure("غير مصرّح.", ErrorType.Forbidden);

            // التحقق من حالة الاشتراك
            if (subscription.Status == SubscriptionStatus.Cancelled)
                return Result<PaymentRequestSummaryDto>.Failure(
                    "الاشتراك ملغي ولا يمكن تقديم دفعة.", ErrorType.BadRequest);

            // منع تقديم دفعتين معلقتين في نفس الوقت
            var hasPending = await _paymentRepo.HasPendingPaymentAsync(request.SubscriptionId, ct);
            if (hasPending)
                return Result<PaymentRequestSummaryDto>.Failure(
                    "لديك طلب دفع معلق بالفعل. يرجى انتظار مراجعته.", ErrorType.Conflict);

            var amount = new Money(dto.Amount);
            PaymentRequest paymentRequest;

            switch (dto.PaymentMethod)
            {
                // ── VodafoneCash / InstaPay ──────────────────
                case PaymentMethod.VodafoneCash:
                case PaymentMethod.InstaPay:
                    {
                        var uploadResult = await _fileUploadService.UploadAsync(
                            dto.ReceiptImage!, "payment-receipts", UploadContentType.Image, ct);

                        if (!uploadResult.IsSuccess)
                            return Result<PaymentRequestSummaryDto>.Failure(
                                uploadResult.Message, ErrorType.InternalServerError);

                        paymentRequest = PaymentRequest.CreateElectronic(
                            subscriptionId: subscription.Id,
                            generalDonationId: null,
                            amount: amount,
                            method: dto.PaymentMethod,
                            receiptImageUrl: uploadResult.Value.Url,
                            receiptImagePublicId: uploadResult.Value.PublicId,
                            senderPhoneNumber: dto.SenderPhoneNumber!
                        );
                        break;
                    }

                // ── Representative (مندوب) ────────────────────
                case PaymentMethod.Representative:
                    {
                        var area = await _deliveryAreaRepo.GetByIdAsync(dto.DeliveryAreaId!.Value, ct);
                        if (area is null)
                            return Result<PaymentRequestSummaryDto>.Failure(
                                "منطقة التوصيل غير موجودة.", ErrorType.NotFound);

                        var repDetails = new RepresentativeDetails(
                            deliveryAreaId: area.Id,
                            deliveryAreaName: area.Name,
                            contactName: dto.ContactName!,
                            contactPhone: dto.ContactPhone!,
                            address: dto.Address!,
                            notes: dto.RepresentativeNotes
                        );

                        paymentRequest = PaymentRequest.CreateRepresentative(
                            subscriptionId: subscription.Id,
                            generalDonationId: null,
                            amount: amount,
                            repDetails: repDetails
                        );
                        break;
                    }

                // ── Branch (الفرع) ───────────────────────────
                case PaymentMethod.Branch:
                    {
                        var slot = await _slotRepo.GetByIdAsync(dto.SlotId!.Value, ct);
                        if (slot is null)
                            return Result<PaymentRequestSummaryDto>.Failure(
                                "الموعد المختار غير موجود.", ErrorType.NotFound);

                        if (!slot.HasAvailableCapacity)
                            return Result<PaymentRequestSummaryDto>.Failure(
                                "هذا الموعد ممتلئ، اختر موعداً آخر.", ErrorType.Conflict);

                        // حجز المكان
                        slot.Book();
                        _slotRepo.Update(slot);

                        var branchDetails = new BranchPaymentDetails(
                            donorName: dto.DonorName!,
                            contactNumber: dto.BranchContactPhone!,
                            scheduledDate: slot.SlotDate.Add(slot.OpenFrom),
                            slotId: slot.Id
                        );

                        paymentRequest = PaymentRequest.CreateBranch(
                            subscriptionId: subscription.Id,
                            generalDonationId: null,
                            amount: amount,
                            branchDetails: branchDetails
                        );
                        break;
                    }

                default:
                    return Result<PaymentRequestSummaryDto>.Failure(
                        "طريقة الدفع غير صحيحة.", ErrorType.BadRequest);
            }

            await _paymentRepo.AddAsync(paymentRequest, ct);
            await _paymentRepo.SaveChangesAsync(ct);

            _logger.LogInformation(
                "تم تقديم طلب الدفع: Id={Id} Method={Method}",
                paymentRequest.Id, paymentRequest.Method);

            return Result<PaymentRequestSummaryDto>.Success(
                MapToDto(paymentRequest, dto),
                "تم استلام طلب الدفع بنجاح. سيتم مراجعته من قِبل الفريق.");
        }

        private static PaymentRequestSummaryDto MapToDto(
            PaymentRequest p, SubmitPaymentDto dto) => new(
            Id: p.Id,
            Method: p.Method.ToString(),
            Status: p.Status.ToString(),
            Amount: p.Amount.Amount,
            ReceiptImageUrl: p.ReceiptImageUrl,
            ReceiptImagePublicId: p.ReceiptImagePublicId,
            SenderPhoneNumber: p.SenderPhoneNumber,
            ContactName: p.RepresentativeInfo?.ContactName,
            ContactPhone: p.RepresentativeInfo?.ContactPhone
                         ?? p.BranchDetails?.ContactNumber,
            ScheduledDate: p.BranchDetails?.ScheduledDate,
            RejectionReason: null,
            CreatedOn: p.CreatedOn
        );
    }
}