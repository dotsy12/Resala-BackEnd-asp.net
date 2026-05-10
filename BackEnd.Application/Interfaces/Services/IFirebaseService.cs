namespace BackEnd.Application.Interfaces.Services
{
    public interface IFirebaseService
    {
        Task<string?> SendNotificationAsync(string token, string title, string message, Dictionary<string, string>? data = null);
        Task SendMulticastNotificationAsync(List<string> tokens, string title, string message, Dictionary<string, string>? data = null);
    }
}
