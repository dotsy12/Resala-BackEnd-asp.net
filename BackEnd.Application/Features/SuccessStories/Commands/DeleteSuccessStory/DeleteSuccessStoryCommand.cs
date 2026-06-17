using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.SuccessStories.Commands.DeleteSuccessStory
{
    public record DeleteSuccessStoryCommand(int Id) : IRequest<Result<bool>>;
}
