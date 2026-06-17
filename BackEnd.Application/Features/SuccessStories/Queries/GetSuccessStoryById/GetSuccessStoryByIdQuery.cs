using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SuccessStory;
using MediatR;

namespace BackEnd.Application.Features.SuccessStories.Queries.GetSuccessStoryById
{
    public record GetSuccessStoryByIdQuery(int Id) : IRequest<Result<SuccessStoryDto>>;
}
