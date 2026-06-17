using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SuccessStory;
using MediatR;
using System.Collections.Generic;

namespace BackEnd.Application.Features.SuccessStories.Queries.GetAllSuccessStories
{
    public record GetAllSuccessStoriesQuery() : IRequest<Result<IReadOnlyList<SuccessStoryDto>>>;
}
