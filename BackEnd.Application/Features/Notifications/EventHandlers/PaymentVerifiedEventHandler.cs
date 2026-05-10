using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Notifications.EventHandlers
{
    public class PaymentVerifiedEventHandler : INotificationHandler<PaymentVerifiedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentVerifiedEventHandler> _logger;

        public PaymentVerifiedEventHandler(
            INotificationService notificationService,
            ILogger<PaymentVerifiedEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(PaymentVerifiedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PaymentVerifiedEvent for Payment {PaymentId}", notification.PaymentRequestId);

            await _notificationService.SendToUserAsync(
                notification.DonorId,
                "تم تأكيد عملية الدفع",
                $"شكراً لك! تم تأكيد عملية الدفع بمبلغ {notification.Amount} ج.م بنجاح.",
                NotificationType.PaymentVerified,
                notification.PaymentRequestId
            );
        }
    }
}
