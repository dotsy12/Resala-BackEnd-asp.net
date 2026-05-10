using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Notification;
using BackEnd.Domain.Enums;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;

namespace BackEnd.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IDeviceTokenRepository _tokenRepo;
        private readonly IDonorRepository _donorRepo;
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepo,
            IDeviceTokenRepository tokenRepo,
            IDonorRepository donorRepo,
            IFirebaseService firebaseService,
            ILogger<NotificationService> logger)
        {
            _notificationRepo = notificationRepo;
            _tokenRepo = tokenRepo;
            _donorRepo = donorRepo;
            _firebaseService = firebaseService;
            _logger = logger;
        }

        public async Task SendToUserAsync(int donorId, string title, string message, NotificationType type, int? relatedEntityId = null, Dictionary<string, string>? extraData = null)
        {
            _logger.LogInformation(">>> Starting SendToUserAsync for Donor: {DonorId}, Type: {Type} <<<", donorId, type);

            // 1. Store in database
            var notification = BackEnd.Domain.Entities.Notification.Notification.Create(donorId, type, title, message, relatedEntityId);
            await _notificationRepo.AddAsync(notification);
            await _notificationRepo.SaveChangesAsync();

            _logger.LogInformation("Notification saved to DB with ID: {NotificationId}", notification.Id);

            // 2. Get device tokens
            var tokens = await _tokenRepo.GetTokensByDonorIdAsync(donorId);
            if (tokens == null || !tokens.Any())
            {
                _logger.LogWarning("### PUSH SKIPPED: No FCM tokens found in DB for DonorId: {DonorId}. Verify token registration mapping! ###", donorId);
                return;
            }

            _logger.LogInformation("Found {Count} active tokens for Donor {DonorId}. Initiating FCM send...", tokens.Count, donorId);

            // 3. Prepare Data Payload for Deep Linking
            var data = new Dictionary<string, string>
            {
                { "type", type.ToString() },
                { "notificationId", notification.Id.ToString() }
            };
            if (relatedEntityId.HasValue) data.Add("relatedEntityId", relatedEntityId.Value.ToString());
            
            // Merge extra data
            if (extraData != null)
            {
                foreach (var item in extraData)
                {
                    if (!data.ContainsKey(item.Key)) data.Add(item.Key, item.Value);
                }
            }

            // 4. Send via Firebase
            foreach (var token in tokens)
            {
                try
                {
                    string? messageId = await _firebaseService.SendNotificationAsync(token, title, message, data);
                    _logger.LogInformation("Notification {NotificationId} sent to User {DonorId}. Firebase MessageId: {MessageId}", 
                        notification.Id, donorId, messageId);
                }
                catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered || ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
                {
                    await UnregisterTokenAsync(token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification {NotificationId} to token {Token} for user {DonorId}", 
                        notification.Id, token, donorId);
                }
            }
        }

        public async Task SendBroadcastAsync(string title, string message, NotificationType type, int? relatedEntityId = null, Dictionary<string, string>? extraData = null)
        {
            // 1. Get all Donors
            var donorIds = await _donorRepo.GetAllIdsAsync();
            if (!donorIds.Any()) return;

            // 2. Persist in database for each donor
            // Note: For very large user bases, this should be handled by a background job
            var notifications = donorIds.Select(donorId => 
                BackEnd.Domain.Entities.Notification.Notification.Create(donorId, type, title, message, relatedEntityId))
                .ToList();

            await _notificationRepo.AddRangeAsync(notifications);
            await _notificationRepo.SaveChangesAsync();

            // 3. Get all device tokens
            var allTokens = await _tokenRepo.GetAllAsync();
            var tokenStrings = allTokens.Select(t => t.Token).ToList();
            int tokenCount = tokenStrings.Count;

            if (tokenCount == 0) return;

            var data = new Dictionary<string, string> { { "type", type.ToString() } };
            if (relatedEntityId.HasValue) data.Add("relatedEntityId", relatedEntityId.Value.ToString());

            // Merge extra data
            if (extraData != null)
            {
                foreach (var item in extraData)
                {
                    if (!data.ContainsKey(item.Key)) data.Add(item.Key, item.Value);
                }
            }

            // Multicast limit is 500 tokens per call
            for (int i = 0; i < tokenCount; i += 500)
            {
                var batch = tokenStrings.Skip(i).Take(500).ToList();
                await _firebaseService.SendMulticastNotificationAsync(batch, title, message, data);
            }
            
            _logger.LogInformation("Broadcast notification '{Title}' sent to {Count} tokens and saved for {DonorCount} users.", 
                title, tokenCount, donorIds.Count);
        }

        public async Task MarkAsReadAsync(int notificationId, int donorId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification != null && notification.DonorId == donorId)
            {
                notification.MarkAsRead();
                _notificationRepo.Update(notification);
                await _notificationRepo.SaveChangesAsync();
            }
        }

        public async Task RegisterTokenAsync(int donorId, string token, string? deviceType = null)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length < 20)
            {
                _logger.LogWarning("Attempted to register an invalid FCM token for user {DonorId}: {Token}", donorId, token);
                return; // Or throw an exception if you want to return a bad request
            }

            var existingToken = await _tokenRepo.GetByTokenAsync(token);
            if (existingToken != null)
            {
                if (existingToken.DonorId == donorId)
                {
                    existingToken.UpdateLastUsed();
                    _tokenRepo.Update(existingToken);
                }
                else
                {
                    // Token belongs to another user (maybe device was sold/reassigned)
                    _tokenRepo.Remove(existingToken);
                    var newToken = DeviceToken.Create(donorId, token, deviceType);
                    await _tokenRepo.AddAsync(newToken);
                }
            }
            else
            {
                var newToken = DeviceToken.Create(donorId, token, deviceType);
                await _tokenRepo.AddAsync(newToken);
            }

            await _tokenRepo.SaveChangesAsync();
        }

        public async Task UnregisterTokenAsync(string token)
        {
            var existingToken = await _tokenRepo.GetByTokenAsync(token);
            if (existingToken != null)
            {
                _tokenRepo.Remove(existingToken);
                await _tokenRepo.SaveChangesAsync();
                _logger.LogInformation("Token {Token} unregistered successfully.", token);
            }
        }
    }
}
