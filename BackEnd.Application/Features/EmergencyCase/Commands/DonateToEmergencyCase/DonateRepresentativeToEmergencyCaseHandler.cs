using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.EmergencyCase;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public class DonateRepresentativeToEmergencyCaseHandler
        : IRequestHandler<DonateRepresentativeToEmergencyCaseCommand, Result<EmergencyDonationResponse>>
    {
        private readonly IEmergencyCaseRepository _caseRepo;
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly IDeliveryAreaRepository _deliveryAreaRepo;
        private readonly ILogger<DonateRepresentativeToEmergencyCaseHandler> _logger;

        public DonateRepresentativeToEmergencyCaseHandler(
            IEmergencyCaseRepository caseRepo,
            IPaymentRequestRepository paymentRepo,
            IDeliveryAreaRepository deliveryAreaRepo,
            ILogger<DonateRepresentativeToEmergencyCaseHandler> logger)
        {
            _caseRepo = caseRepo;
            _paymentRepo = paymentRepo;
            _deliveryAreaRepo = deliveryAreaRepo;
            _logger = logger;
        }

        public async Task<Result<EmergencyDonationResponse>> Handle(
            DonateRepresentativeToEmergencyCaseCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            var emergencyCase = await _caseRepo.GetByIdAsync(request.CaseId, ct);
            if (emergencyCase is null)
                return Result<EmergencyDonationResponse>.Failure("حالة الطوارئ غير موجودة.", ErrorType.NotFound);

            if (!emergencyCase.IsActive)
                return Result<EmergencyDonationResponse>.Failure("هذه الحالة غير نشطة حالياً.", ErrorType.BadRequest);

            var area = await _deliveryAreaRepo.GetByIdAsync(dto.DeliveryAreaId, ct);
            if (area is null)
                return Result<EmergencyDonationResponse>.Failure("منطقة التوصيل غير موجودة.", ErrorType.NotFound);

            var repDetails = new RepresentativeDetails(
                deliveryAreaId: area.Id,
                deliveryAreaName: area.Name,
                contactName: dto.ContactName,
                contactPhone: dto.ContactPhone,
                address: dto.Address,
                notes: dto.RepresentativeNotes
            );

            var paymentRequest = PaymentRequest.CreateRepresentative(
                donorId: request.DonorId,
                subscriptionId: null,
                emergencyCaseId: emergencyCase.Id,
                generalDonationId: null,
                amount: new Money(dto.Amount),
                repDetails: repDetails
            );

            await _paymentRepo.AddAsync(paymentRequest, ct);
            await _paymentRepo.SaveChangesAsync(ct);

            _logger.LogInformation("تم تقديم طلب تبرع مندوب لحالة طوارئ: Id={Id}, Case={CaseId}", paymentRequest.Id, emergencyCase.Id);

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
