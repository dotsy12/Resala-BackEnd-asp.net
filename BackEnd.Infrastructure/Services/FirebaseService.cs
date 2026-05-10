using BackEnd.Application.Interfaces.Services;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEnd.Infrastructure.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly ILogger<FirebaseService> _logger;
        private readonly FirebaseMessaging _messaging;

        public FirebaseService(ILogger<FirebaseService> logger)
        {
            _logger = logger;
            _messaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task<string?> SendNotificationAsync(string token, string title, string message, Dictionary<string, string>? data = null)
        {
            try
            {
                var msg = new Message()
                {
                    Token = token,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = message,
                    },
                    Data = data
                };

                string messageId = await _messaging.SendAsync(msg);
                _logger.LogInformation("Successfully sent FCM message. Firebase MessageId: {MessageId}", messageId);
                return messageId;
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered || ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
            {
                _logger.LogWarning("FCM Token is invalid or unregistered: {Token}. Error: {Error}", token, ex.Message);
                throw; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending FCM notification to token {Token}", token);
                return null;
            }
        }

        public async Task SendMulticastNotificationAsync(List<string> tokens, string title, string message, Dictionary<string, string>? data = null)
        {
            if (tokens == null || !tokens.Any()) return;

            try
            {
                var msg = new MulticastMessage()
                {
                    Tokens = tokens,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = message,
                    },
                    Data = data
                };

                // Fix: SendMulticastAsync used the deprecated /batch endpoint which was shut down by Google.
                // SendEachForMulticastAsync sends individual messages under the hood.
                var response = await _messaging.SendEachForMulticastAsync(msg);
                _logger.LogInformation("Successfully sent FCM multicast message. Success: {Success}, Failure: {Failure}", response.SuccessCount, response.FailureCount);
                
                if (response.FailureCount > 0)
                {
                    for (var i = 0; i < response.Responses.Count; i++)
                    {
                        if (!response.Responses[i].IsSuccess)
                        {
                            var errorCode = response.Responses[i].Exception?.MessagingErrorCode;
                            if (errorCode == MessagingErrorCode.Unregistered || errorCode == MessagingErrorCode.InvalidArgument)
                            {
                                _logger.LogWarning("FCM Token is invalid: {Token}", tokens[i]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending FCM multicast notification");
            }
        }
    }
}
