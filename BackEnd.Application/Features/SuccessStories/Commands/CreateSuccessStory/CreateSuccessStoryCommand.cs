using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SuccessStory;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BackEnd.Application.Features.SuccessStories.Commands.CreateSuccessStory
{
    public record CreateSuccessStoryCommand(
        string Title,
        string Description,
        IFormFile Image
    ) : IRequest<Result<SuccessStoryDto>>;
}
