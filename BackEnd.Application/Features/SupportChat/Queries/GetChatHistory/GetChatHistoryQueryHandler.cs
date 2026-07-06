using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SupportChat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.SupportChat.Queries.GetChatHistory
{
    public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, Result<PagedResult<SupportMessageDto>>>
    {
        private readonly ISupportMessageRepository _repository;

        public GetChatHistoryQueryHandler(ISupportMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<PagedResult<SupportMessageDto>>> Handle(GetChatHistoryQuery request, CancellationToken ct)
        {
            var (items, total) = await _repository.GetPagedMessagesAsync(
                request.ChatOwnerUserId, request.PageNumber, request.PageSize, ct);

            var dtoList = items.Select(m => new SupportMessageDto
            {
                Id = m.Id,
                MessageText = m.MessageText,
                CreatedOn = m.CreatedOn,
                IsRead = m.IsRead,
                SenderName = m.SenderName,
                IsFromStaff = m.IsFromStaff,
                ChatOwnerUserId = m.ChatOwnerUserId
            }).ToList();

            var pagedResult = new PagedResult<SupportMessageDto>(
                dtoList, total, request.PageNumber, request.PageSize);

            return Result<PagedResult<SupportMessageDto>>.Success(pagedResult);
        }
    }
}
