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
    public class DonateBranchToEmergencyCaseHandler
        : IRequestHandler<DonateBranchToEmergencyCaseCommand, Result<EmergencyDonationResponse>>
    {
        private readonly IEmergencyCaseRepository _caseRepo;
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly IAppointmentSlotRepository _slotRepo;
        private readonly IDonorRepository _donorRepo;
        private readonly ILogger<DonateBranchToEmergencyCaseHandler> _logger;

        public DonateBranchToEmergencyCaseHandler(
            IEmergencyCaseRepository caseRepo,
            IPaymentRequestRepository paymentRepo,
            IAppointmentSlotRepository slotRepo,
            IDonorRepository donorRepo,
            ILogger<DonateBranchToEmergencyCaseHandler> logger)
        {
            _caseRepo = caseRepo;
            _paymentRepo = paymentRepo;
            _slotRepo = slotRepo;
            _donorRepo = donorRepo;
            _logger = logger;
        }

        public async Task<Result<EmergencyDonationResponse>> Handle(
            DonateBranchToEmergencyCaseCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            var emergencyCase = await _caseRepo.GetByIdAsync(request.CaseId, ct);
            if (emergencyCase is null)
                return Result<EmergencyDonationResponse>.Failure("حالة الطوارئ غير موجودة.", ErrorType.NotFound);

            if (!emergencyCase.IsActive)
                return Result<EmergencyDonationResponse>.Failure("هذه الحالة غير نشطة حالياً.", ErrorType.BadRequest);

            var slot = await _slotRepo.GetByIdAsync(dto.SlotId, ct);
            if (slot is null)
                return Result<EmergencyDonationResponse>.Failure("الموعد المختار غير موجود.", ErrorType.NotFound);

            if (!slot.HasAvailableCapacity)
                return Result<EmergencyDonationResponse>.Failure("هذا الموعد ممتلئ، اختر موعداً آخر.", ErrorType.Conflict);

            var donor = await _donorRepo.GetByIdAsync(request.DonorId, ct);
            var donorName = donor?.FullName?.ToString() ?? "متبرع";

            slot.Book();
            _slotRepo.Update(slot);

            var branchDetails = new BranchPaymentDetails(
                donorName: donorName,
                contactNumber: dto.BranchContactPhone,
                scheduledDate: slot.SlotDate.Add(slot.OpenFrom),
                slotId: slot.Id
            );

            var paymentRequest = PaymentRequest.CreateBranch(
                donorId: request.DonorId,
                subscriptionId: null,
                emergencyCaseId: emergencyCase.Id,
                generalDonationId: null,
                amount: new Money(dto.Amount),
                branchDetails: branchDetails
            );

            await _paymentRepo.AddAsync(paymentRequest, ct);
            await _paymentRepo.SaveChangesAsync(ct);

            _logger.LogInformation("تم تقديم طلب تبرع فرع لحالة طوارئ: Id={Id}, Case={CaseId}", paymentRequest.Id, emergencyCase.Id);

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
