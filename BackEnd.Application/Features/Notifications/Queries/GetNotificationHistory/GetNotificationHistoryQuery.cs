using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Notification;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Notifications.Queries.GetNotificationHistory
{
    public record GetNotificationHistoryQuery(int DonorId) : IRequest<Result<List<NotificationDto>>>;

    public class GetNotificationHistoryHandler : IRequestHandler<GetNotificationHistoryQuery, Result<List<NotificationDto>>>
    {
        private readonly INotificationRepository _notificationRepo;

        public GetNotificationHistoryHandler(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task<Result<List<NotificationDto>>> Handle(GetNotificationHistoryQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _notificationRepo.GetByDonorIdAsync(request.DonorId, cancellationToken);
            
            var dtos = notifications.Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.Type.ToString(),
                n.IsRead,
                n.CreatedOn,
                n.RelatedEntityId
            )).ToList();

            return Result<List<NotificationDto>>.Success(dtos);
        }
    }
}
