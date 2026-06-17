using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.ValueObjects;
using MediatR;

namespace BackEnd.Application.Features.Admin.DirectOperations.Commands.ProcessDirectPayment
{
    public class ProcessDirectPaymentHandler : IRequestHandler<ProcessDirectPaymentCommand, Result<int>>
    {
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly IEmergencyCaseRepository _emergencyRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessDirectPaymentHandler(
            IPaymentRequestRepository paymentRepo,
            ISponsorshipSubscriptionRepository subRepo,
            IEmergencyCaseRepository emergencyRepo,
            IUnitOfWork unitOfWork)
        {
            _paymentRepo = paymentRepo;
            _subRepo = subRepo;
            _emergencyRepo = emergencyRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(ProcessDirectPaymentCommand request, CancellationToken ct)
        {
            // 1. Create the direct branch payment
            var payment = PaymentRequest.CreateDirectBranch(
                request.DonorId,
                request.SubscriptionId,
                request.EmergencyCaseId,
                request.GeneralDonationId,
                new Money(request.Amount),
                request.StaffId
            );

            // 2. Immediately Verify (Staff initiated)
            payment.Verify(request.StaffId);

            // 3. Update related entities
            if (payment.SubscriptionId.HasValue)
            {
                var sub = await _subRepo.GetByIdAsync(payment.SubscriptionId.Value, ct);
                if (sub != null)
                {
                    sub.AdvancePaymentDate();
                    _subRepo.Update(sub);
                }
            }

            if (payment.EmergencyCaseId.HasValue)
            {
                var emergencyCase = await _emergencyRepo.GetByIdTrackedAsync(payment.EmergencyCaseId.Value, ct);
                if (emergencyCase != null)
                {
                    emergencyCase.AddDonation(payment.Amount);
                    await _emergencyRepo.UpdateAsync(emergencyCase, ct);
                }
            }

            await _paymentRepo.AddAsync(payment, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<int>.Success(payment.Id, "تم تسجيل الدفع المباشر في الفرع بنجاح.");
        }
    }
}