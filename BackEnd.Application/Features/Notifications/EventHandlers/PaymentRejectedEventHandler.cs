using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Notifications.EventHandlers
{
    public class PaymentRejectedEventHandler : INotificationHandler<PaymentRejectedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentRejectedEventHandler> _logger;

        public PaymentRejectedEventHandler(
            INotificationService notificationService,
            ILogger<PaymentRejectedEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(PaymentRejectedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PaymentRejectedEvent for Payment {PaymentId}", notification.PaymentRequestId);

            var extraData = new Dictionary<string, string>
            {
                { "rejectionReason", notification.Reason }
            };

            await _notificationService.SendToUserAsync(
                notification.DonorId,
                "تم رفض عملية الدفع",
                $"نعتذر، تم رفض عملية الدفع. السبب: {notification.Reason}",
                NotificationType.PaymentRejected,
                notification.PaymentRequestId,
                extraData
            );
        }
    }
}
