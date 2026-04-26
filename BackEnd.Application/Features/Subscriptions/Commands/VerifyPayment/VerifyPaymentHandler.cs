// VerifyPaymentCommand.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.VerifyPayment
{
    public class VerifyPaymentHandler
        : IRequestHandler<VerifyPaymentCommand, Result<string>>
    {
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly IEmergencyCaseRepository _emergencyRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly ILogger<VerifyPaymentHandler> _logger;

        public VerifyPaymentHandler(
            IPaymentRequestRepository paymentRepo,
            ISponsorshipSubscriptionRepository subRepo,
            IEmergencyCaseRepository emergencyRepo,
            INotificationRepository notificationRepo,
            ILogger<VerifyPaymentHandler> logger)
        { 
            _paymentRepo = paymentRepo; 
            _subRepo = subRepo; 
            _emergencyRepo = emergencyRepo;
            _notificationRepo = notificationRepo;
            _logger = logger; 
        }

        public async Task<Result<string>> Handle(
            VerifyPaymentCommand request, CancellationToken ct)
        {
            var payment = await _paymentRepo.GetByIdAsync(request.PaymentId, ct);
            if (payment is null)
                return Result<string>.Failure("طلب الدفع غير موجود.", ErrorType.NotFound);

            payment.Verify(request.StaffId);
            _paymentRepo.Update(payment);

            // 1. إذا كان الدفع لاشتراك (Sponsorship)
            if (payment.SubscriptionId.HasValue)
            {
                var sub = await _subRepo.GetByIdAsync(payment.SubscriptionId.Value, ct);
                if (sub is not null)
                {
                    sub.AdvancePaymentDate();
                    _subRepo.Update(sub);
                }
            }
            
            // 2. إذا كان الدفع لحالة طوارئ (Emergency Case)
            if (payment.EmergencyCaseId.HasValue)
            {
                var emergencyCase = await _emergencyRepo.GetByIdTrackedAsync(payment.EmergencyCaseId.Value, ct);
                if (emergencyCase is not null)
                {
                    emergencyCase.AddDonation(payment.Amount);
                    await _emergencyRepo.UpdateAsync(emergencyCase, ct);
                }
            }

            // 3. إرسال إشعار للمتبرع
            var notification = BackEnd.Domain.Entities.Notification.Notification.Create(
                donorId: payment.DonorId,
                type: NotificationType.PaymentVerified,
                title: "تم تأكيد عملية الدفع",
                message: $"شكراً لك! تم تأكيد عملية الدفع بمبلغ {payment.Amount.Amount} ج.م بنجاح.",
                relatedEntityId: payment.Id
            );
            await _notificationRepo.AddAsync(notification, ct);

            await _paymentRepo.SaveChangesAsync(ct);

            _logger.LogInformation(
                "تم تأكيد الدفع: Id={Id} بواسطة Staff={StaffId}",
                payment.Id, request.StaffId);

            return Result<string>.Success("تم تأكيد الدفع بنجاح وتحديث البيانات المتعلقة وإرسال إشعار للمتبرع.");
        }
    }
}
