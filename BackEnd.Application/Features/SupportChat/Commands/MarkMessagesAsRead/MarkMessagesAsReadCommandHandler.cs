using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.SupportChat.Commands.MarkMessagesAsRead
{
    public class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand, Result<bool>>
    {
        private readonly ISupportMessageRepository _repository;
        private readonly ILogger<MarkMessagesAsReadCommandHandler> _logger;

        public MarkMessagesAsReadCommandHandler(
            ISupportMessageRepository repository,
            ILogger<MarkMessagesAsReadCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(MarkMessagesAsReadCommand request, CancellationToken ct)
        {
            var unreadMessages = await _repository.GetUnreadMessagesAsync(request.ChatOwnerUserId, ct);

            if (unreadMessages.Count > 0)
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.UpdatedOn = DateTime.UtcNow;
                }

                await _repository.SaveChangesAsync(ct);
                _logger.LogInformation("Marked {Count} support messages as read for User {ChatOwnerUserId}", unreadMessages.Count, request.ChatOwnerUserId);
            }

            return Result<bool>.Success(true, "تم تحديد الرسائل كمقروءة بنجاح.");
        }
    }
}
