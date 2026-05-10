using BackEnd.Domain.Enums;

namespace BackEnd.Application.Dtos.Notification
{
    public record NotificationDto(
        int Id,
        string Title,
        string Message,
        string Type,
        bool IsRead,
        DateTime CreatedOn,
        int? RelatedEntityId);

    public record RegisterDeviceTokenDto(
        string Token,
        string? DeviceType);

    public record BroadcastNotificationDto(
        string Title,
        string Message,
        NotificationType Type,
        int? RelatedEntityId = null);
}
