using BackEnd.Domain.Enums;

namespace BackEnd.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendToUserAsync(int donorId, string title, string message, NotificationType type, int? relatedEntityId = null, Dictionary<string, string>? extraData = null);
        Task SendBroadcastAsync(string title, string message, NotificationType type, int? relatedEntityId = null, Dictionary<string, string>? extraData = null);
        Task RegisterTokenAsync(int donorId, string token, string? deviceType = null);
        Task UnregisterTokenAsync(string token);
        Task MarkAsReadAsync(int notificationId, int donorId);
    }
}
