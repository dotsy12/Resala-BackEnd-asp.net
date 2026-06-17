using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Feedback;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Feedbacks.Queries.GetAllFeedbacks
{
    public record GetAllFeedbacksQuery(
        FeedbackType? Type = null,
        FeedbackStatus? Status = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Result<PagedResult<FeedbackDto>>>;
}
