using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Notifications.Commands.SendBroadcastNotification
{
    public record SendBroadcastNotificationCommand(
        string Title,
        string Message,
        NotificationType Type,
        int? RelatedEntityId = null) : IRequest<Result<bool>>;

    public class SendBroadcastNotificationHandler : IRequestHandler<SendBroadcastNotificationCommand, Result<bool>>
    {
        private readonly INotificationService _notificationService;

        public SendBroadcastNotificationHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<Result<bool>> Handle(SendBroadcastNotificationCommand request, CancellationToken cancellationToken)
        {
            await _notificationService.SendBroadcastAsync(request.Title, request.Message, request.Type, request.RelatedEntityId);
            return Result<bool>.Success(true);
        }
    }
}
