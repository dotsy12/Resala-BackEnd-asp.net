using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.SupportChat.Commands.MarkMessagesAsRead
{
    public record MarkMessagesAsReadCommand(string ChatOwnerUserId) : IRequest<Result<bool>>;
}
