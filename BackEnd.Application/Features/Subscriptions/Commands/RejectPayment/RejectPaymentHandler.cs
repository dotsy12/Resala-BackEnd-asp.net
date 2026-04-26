// RejectPaymentCommand.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.RejectPayment
{
    public class RejectPaymentHandler
        : IRequestHandler<RejectPaymentCommand, Result<string>>
    {
        private readonly IPaymentRequestRepository _repo;
        private readonly INotificationRepository _notificationRepo;
        private readonly ILogger<RejectPaymentHandler> _logger;

        public RejectPaymentHandler(
            IPaymentRequestRepository repo,
            INotificationRepository notificationRepo,
            ILogger<RejectPaymentHandler> logger)
        { 
            _repo = repo; 
            _notificationRepo = notificationRepo;
            _logger = logger; 
        }

        public async Task<Result<string>> Handle(
            RejectPaymentCommand request, CancellationToken ct)
        {
            var payment = await _repo.GetByIdAsync(request.PaymentId, ct);
            if (payment is null)
                return Result<string>.Failure("طلب الدفع غير موجود.", ErrorType.NotFound);

            payment.Reject(request.StaffId, request.Reason);
            _repo.Update(payment);

            // إرسال إشعار للمتبرع بالرفض
            var notification = BackEnd.Domain.Entities.Notification.Notification.Create(
                donorId: payment.DonorId,
                type: NotificationType.PaymentVerified, // استخدام نوع مناسب أو إضافة نوع جديد
                title: "تم رفض عملية الدفع",
                message: $"نعتذر، تم رفض عملية الدفع بمبلغ {payment.Amount.Amount} ج.م. السبب: {request.Reason}",
                relatedEntityId: payment.Id
            );
            await _notificationRepo.AddAsync(notification, ct);

            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("تم رفض الدفع: Id={Id}", payment.Id);
            return Result<string>.Success("تم رفض طلب الدفع وإرسال إشعار للمتبرع.");
        }
    }
}
