using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SupportChat;
using MediatR;

namespace BackEnd.Application.Features.SupportChat.Queries.GetChatHistory
{
    public record GetChatHistoryQuery(
        string ChatOwnerUserId,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<Result<PagedResult<SupportMessageDto>>>;
}
