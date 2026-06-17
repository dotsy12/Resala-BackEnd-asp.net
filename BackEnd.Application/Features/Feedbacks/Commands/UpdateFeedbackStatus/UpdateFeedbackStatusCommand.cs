using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus
{
    public record UpdateFeedbackStatusCommand(int Id, FeedbackStatus Status) : IRequest<Result<bool>>;
}
