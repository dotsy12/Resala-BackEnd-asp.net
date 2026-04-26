using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.EmergencyCase;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public class DonateElectronicToEmergencyCaseHandler
        : IRequestHandler<DonateElectronicToEmergencyCaseCommand, Result<EmergencyDonationResponse>>
    {
        private readonly IEmergencyCaseRepository _caseRepo;
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<DonateElectronicToEmergencyCaseHandler> _logger;

        public DonateElectronicToEmergencyCaseHandler(
            IEmergencyCaseRepository caseRepo,
            IPaymentRequestRepository paymentRepo,
            IFileUploadService fileUploadService,
            ILogger<DonateElectronicToEmergencyCaseHandler> _logger)
        {
            _caseRepo = caseRepo;
            _paymentRepo = paymentRepo;
            _fileUploadService = fileUploadService;
            this._logger = _logger;
        }

        public async Task<Result<EmergencyDonationResponse>> Handle(
            DonateElectronicToEmergencyCaseCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            var emergencyCase = await _caseRepo.GetByIdAsync(request.CaseId, ct);
            if (emergencyCase is null)
                return Result<EmergencyDonationResponse>.Failure("حالة الطوارئ غير موجودة.", ErrorType.NotFound);

            if (!emergencyCase.IsActive)
                return Result<EmergencyDonationResponse>.Failure("هذه الحالة غير نشطة حالياً.", ErrorType.BadRequest);

            var uploadResult = await _fileUploadService.UploadAsync(
                dto.ReceiptImage, "payment-receipts", UploadContentType.Image, ct);

            if (!uploadResult.IsSuccess)
                return Result<EmergencyDonationResponse>.Failure(uploadResult.Message, ErrorType.InternalServerError);

            var paymentRequest = PaymentRequest.CreateElectronic(
                donorId: request.DonorId,
                subscriptionId: null,
                emergencyCaseId: emergencyCase.Id,
                generalDonationId: null,
                amount: new Money(dto.Amount),
                method: (PaymentMethod)dto.PaymentMethod,
                receiptImageUrl: uploadResult.Value.Url,
                receiptImagePublicId: uploadResult.Value.PublicId,
                senderPhoneNumber: dto.SenderPhoneNumber
            );

            await _paymentRepo.AddAsync(paymentRequest, ct);
            await _paymentRepo.SaveChangesAsync(ct);

            _logger.LogInformation("تم تقديم طلب تبرع إلكتروني لحالة طوارئ: Id={Id}, Case={CaseId}", paymentRequest.Id, emergencyCase.Id);

            var response = new EmergencyDonationResponse(
                PaymentId: paymentRequest.Id,
                CaseId: emergencyCase.Id,
                Amount: paymentRequest.Amount.Amount,
                Method: paymentRequest.Method.ToString(),
                Status: paymentRequest.Status.ToString()
            );

            return Result<EmergencyDonationResponse>.Success(response, "تم إرسال طلب التبرع بنجاح، وسيتم مراجعته.");
        }
    }
}
