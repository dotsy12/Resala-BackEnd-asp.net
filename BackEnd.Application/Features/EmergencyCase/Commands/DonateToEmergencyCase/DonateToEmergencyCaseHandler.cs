using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public class DonateToEmergencyCaseHandler
        : IRequestHandler<DonateToEmergencyCaseCommand, Result<EmergencyDonationResponse>>
    {
        private readonly IEmergencyCaseRepository _caseRepo;
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly IDeliveryAreaRepository _deliveryAreaRepo;
        private readonly IAppointmentSlotRepository _slotRepo;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<DonateToEmergencyCaseHandler> _logger;

        public DonateToEmergencyCaseHandler(
            IEmergencyCaseRepository caseRepo,
            IPaymentRequestRepository paymentRepo,
            IDeliveryAreaRepository deliveryAreaRepo,
            IAppointmentSlotRepository slotRepo,
            IFileUploadService fileUploadService,
            ILogger<DonateToEmergencyCaseHandler> logger)
        {
            _caseRepo = caseRepo;
            _paymentRepo = paymentRepo;
            _deliveryAreaRepo = deliveryAreaRepo;
            _slotRepo = slotRepo;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Result<EmergencyDonationResponse>> Handle(
            DonateToEmergencyCaseCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            var emergencyCase = await _caseRepo.GetByIdAsync(request.CaseId, ct);
            if (emergencyCase is null)
                return Result<EmergencyDonationResponse>.Failure("حالة الطوارئ غير موجودة.", ErrorType.NotFound);

            if (!emergencyCase.IsActive)
                return Result<EmergencyDonationResponse>.Failure("هذه الحالة غير نشطة حالياً.", ErrorType.BadRequest);

            var amount = new Money(dto.Amount);
            PaymentRequest paymentRequest;

            switch (dto.PaymentMethod)
            {
                case PaymentMethod.VodafoneCash:
                case PaymentMethod.InstaPay:
                    {
                        var uploadResult = await _fileUploadService.UploadAsync(
                            dto.ReceiptImage!, "payment-receipts", UploadContentType.Image, ct);

                        if (!uploadResult.IsSuccess)
                            return Result<EmergencyDonationResponse>.Failure(uploadResult.Message, ErrorType.InternalServerError);

                        paymentRequest = PaymentRequest.CreateElectronic(
                            donorId: request.DonorId,
                            subscriptionId: null,
                            emergencyCaseId: emergencyCase.Id,
                            generalDonationId: null,
                            amount: amount,
                            method: dto.PaymentMethod,
                            receiptImageUrl: uploadResult.Value.Url,
                            receiptImagePublicId: uploadResult.Value.PublicId,
                            senderPhoneNumber: dto.SenderPhoneNumber!
                        );
                        break;
                    }

                case PaymentMethod.Representative:
                    {
                        var area = await _deliveryAreaRepo.GetByIdAsync(dto.DeliveryAreaId!.Value, ct);
                        if (area is null)
                            return Result<EmergencyDonationResponse>.Failure("منطقة التوصيل غير موجودة.", ErrorType.NotFound);

                        var repDetails = new RepresentativeDetails(
                            deliveryAreaId: area.Id,
                            deliveryAreaName: area.Name,
                            contactName: dto.ContactName!,
                            contactPhone: dto.ContactPhone!,
                            address: dto.Address!,
                            notes: dto.RepresentativeNotes
                        );

                        paymentRequest = PaymentRequest.CreateRepresentative(
                            donorId: request.DonorId,
                            subscriptionId: null,
                            emergencyCaseId: emergencyCase.Id,
                            generalDonationId: null,
                            amount: amount,
                            repDetails: repDetails
                        );
                        break;
                    }

                case PaymentMethod.Branch:
                    {
                        var slot = await _slotRepo.GetByIdAsync(dto.SlotId!.Value, ct);
                        if (slot is null)
                            return Result<EmergencyDonationResponse>.Failure("الموعد المختار غير موجود.", ErrorType.NotFound);

                        if (!slot.HasAvailableCapacity)
                            return Result<EmergencyDonationResponse>.Failure("هذا الموعد ممتلئ، اختر موعداً آخر.", ErrorType.Conflict);

                        slot.Book();
                        _slotRepo.Update(slot);

                        var branchDetails = new BranchPaymentDetails(
                            donorName: dto.DonorName!,
                            contactNumber: dto.BranchContactPhone!,
                            scheduledDate: slot.SlotDate.Add(slot.OpenFrom),
                            slotId: slot.Id
                        );

                        paymentRequest = PaymentRequest.CreateBranch(
                            donorId: request.DonorId,
                            subscriptionId: null,
                            emergencyCaseId: emergencyCase.Id,
                            generalDonationId: null,
                            amount: amount,
                            branchDetails: branchDetails
                        );
                        break;
                    }

                default:
                    return Result<EmergencyDonationResponse>.Failure("طريقة الدفع غير صحيحة.", ErrorType.BadRequest);
            }

            await _paymentRepo.AddAsync(paymentRequest, ct);
            await _paymentRepo.SaveChangesAsync(ct);

            _logger.LogInformation("تم تقديم طلب تبرع لحالة طوارئ: Id={Id}, Case={CaseId}", paymentRequest.Id, emergencyCase.Id);

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
