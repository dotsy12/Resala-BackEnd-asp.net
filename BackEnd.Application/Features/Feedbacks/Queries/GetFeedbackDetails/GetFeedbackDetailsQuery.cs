using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Feedback;
using MediatR;

namespace BackEnd.Application.Features.Feedbacks.Queries.GetFeedbackDetails
{
    public record GetFeedbackDetailsQuery(int Id) : IRequest<Result<FeedbackDto>>;
}
