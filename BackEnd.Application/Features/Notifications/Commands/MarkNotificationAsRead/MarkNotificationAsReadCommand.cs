using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using MediatR;

namespace BackEnd.Application.Features.Notifications.Commands.MarkNotificationAsRead
{
    public record MarkNotificationAsReadCommand(int NotificationId, int DonorId) : IRequest<Result<bool>>;

    public class MarkNotificationAsReadHandler : IRequestHandler<MarkNotificationAsReadCommand, Result<bool>>
    {
        private readonly INotificationService _notificationService;

        public MarkNotificationAsReadHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<Result<bool>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            await _notificationService.MarkAsReadAsync(request.NotificationId, request.DonorId);
            return Result<bool>.Success(true);
        }
    }
}
