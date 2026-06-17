using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Feedback;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Feedbacks.Commands.SubmitFeedback
{
    public record SubmitFeedbackCommand(
        string Subject,
        string Message,
        FeedbackType Type
    ) : IRequest<Result<FeedbackDto>>;
}
