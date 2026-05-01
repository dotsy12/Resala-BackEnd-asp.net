using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Application.Common.Files;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.SubmitPayment
{
    public record SubmitElectronicSubscriptionPaymentCommand(
        int SubscriptionId,
        int DonorId,
        decimal Amount,
        PaymentMethod Method,
        string SenderPhoneNumber,
        IFormFile ReceiptImage
    ) : IRequest<Result<PaymentRequestSummaryDto>>;

    public class SubmitElectronicSubscriptionPaymentHandler
        : IRequestHandler<SubmitElectronicSubscriptionPaymentCommand, Result<PaymentRequestSummaryDto>>
    {
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<SubmitElectronicSubscriptionPaymentHandler> _logger;

        public SubmitElectronicSubscriptionPaymentHandler(
            ISponsorshipSubscriptionRepository subRepo,
            IPaymentRequestRepository paymentRepo,
            IFileUploadService fileUploadService,
            ILogger<SubmitElectronicSubscriptionPaymentHandler> logger)
        {
            _subRepo = subRepo;
            _paymentRepo = paymentRepo;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Result<PaymentRequestSummaryDto>> Handle(
            SubmitElectronicSubscriptionPaymentCommand request, CancellationToken ct)
        {
            _logger.LogInformation(
                "تقديم دفع إلكتروني لاشتراك — SubscriptionId:{SubId} Method:{Method} DonorId:{DonorId}",
                request.SubscriptionId, request.Method, request.DonorId);

            var subscription = await _subRepo.GetByIdAsync(request.SubscriptionId, ct);
            if (subscription is null)
                return Result<PaymentRequestSummaryDto>.Failure("الاشتراك غير موجود.", ErrorType.NotFound);

            if (subscription.DonorId != request.DonorId)
                return Result<PaymentRequestSummaryDto>.Failure("غير مصرّح.", ErrorType.Forbidden);

            if (subscription.Status == SubscriptionStatus.Cancelled)
                return Result<PaymentRequestSummaryDto>.Failure("الاشتراك ملغي.", ErrorType.BadRequest);

            var hasPending = await _paymentRepo.HasPendingPaymentAsync(request.SubscriptionId, ct);
            if (hasPending)
                return Result<PaymentRequestSummaryDto>.Failure("لديك طلب دفع معلق بالفعل.", ErrorType.Conflict);

            var uploadResult = await _fileUploadService.UploadAsync(
                request.ReceiptImage, "payment-receipts", UploadContentType.Image, ct);

            if (!uploadResult.IsSuccess)
                return Result<PaymentRequestSummaryDto>.Failure(uploadResult.Message, ErrorType.InternalServerError);

            var paymentRequest = PaymentRequest.CreateElectronic(
                donorId: request.DonorId,
                subscriptionId: subscription.Id,
                emergencyCaseId: null,
                generalDonationId: null,
                amount: new Money(request.Amount),
                method: request.Method,
                receiptImageUrl: uploadResult.Value.Url,
                receiptImagePublicId: uploadResult.Value.PublicId,
                senderPhoneNumber: request.SenderPhoneNumber
            );

            await _paymentRepo.AddAsync(paymentRequest, ct);
            await _paymentRepo.SaveChangesAsync(ct);

            return Result<PaymentRequestSummaryDto>.Success(
                MapToDto(paymentRequest), "تم استلام طلب الدفع الإلكتروني بنجاح.");
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
            ReceiptImageUrl: p.ReceiptImageUrl,
            ReceiptImagePublicId: p.ReceiptImagePublicId,
            SenderPhoneNumber: p.SenderPhoneNumber,
            ContactName: null,
            ContactPhone: null,
            Address: null,
            RepresentativeNotes: null,
            DeliveryAreaId: null,
            DeliveryAreaName: null,
            ScheduledDate: null,
            RejectionReason: null,
            CreatedOn: p.CreatedOn
        );
    }
}