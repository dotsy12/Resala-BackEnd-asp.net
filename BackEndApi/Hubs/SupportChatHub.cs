using BackEnd.Application.Dtos.SupportChat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackEnd.Api.Hubs
{
    [Authorize]
    public class SupportChatHub : Hub
    {
        private readonly ISupportMessageRepository _repository;

        public SupportChatHub(ISupportMessageRepository repository)
        {
            _repository = repository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var isStaff = Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("Reception") == true;

                if (isStaff)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Group_Staff");
                }
                else
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Group_User_" + userId);
                }
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessageToChat(string chatOwnerUserId, string messageText)
        {
            var senderId = Context.UserIdentifier ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId))
            {
                throw new HubException("User is not authenticated.");
            }

            var isStaff = Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("Reception") == true;
            var senderName = Context.User?.FindFirst("fullName")?.Value ?? Context.User?.Identity?.Name ?? "Support Staff";

            if (!isStaff)
            {
                senderName = Context.User?.FindFirst("fullName")?.Value ?? Context.User?.Identity?.Name ?? "Donor";
            }

            var supportMessage = SupportMessage.Create(
                messageText,
                senderId,
                senderName,
                isStaff,
                chatOwnerUserId
            );

            await _repository.AddAsync(supportMessage);
            await _repository.SaveChangesAsync();

            var dto = new SupportMessageDto
            {
                Id = supportMessage.Id,
                MessageText = supportMessage.MessageText,
                CreatedOn = supportMessage.CreatedOn,
                IsRead = supportMessage.IsRead,
                SenderName = supportMessage.SenderName,
                IsFromStaff = supportMessage.IsFromStaff,
                ChatOwnerUserId = supportMessage.ChatOwnerUserId
            };

            if (isStaff)
            {
                // Send to the client
                await Clients.Group("Group_User_" + chatOwnerUserId).SendAsync("ReceiveMessage", dto);
                // Also broadcast to staff group so other staff members see the reply
                await Clients.Group("Group_Staff").SendAsync("ReceiveMessage", dto);
            }
            else
            {
                // Send to the staff group
                await Clients.Group("Group_Staff").SendAsync("ReceiveMessage", dto);
                // Also send back to user's group in case they have multiple devices
                await Clients.Group("Group_User_" + chatOwnerUserId).SendAsync("ReceiveMessage", dto);
            }
        }
    }
}
