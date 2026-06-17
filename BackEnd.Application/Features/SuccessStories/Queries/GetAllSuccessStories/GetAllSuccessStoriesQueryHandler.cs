using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SuccessStory;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.SuccessStories.Queries.GetAllSuccessStories
{
    public class GetAllSuccessStoriesQueryHandler : IRequestHandler<GetAllSuccessStoriesQuery, Result<IReadOnlyList<SuccessStoryDto>>>
    {
        private readonly ISuccessStoryRepository _repository;

        public GetAllSuccessStoriesQueryHandler(ISuccessStoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<SuccessStoryDto>>> Handle(GetAllSuccessStoriesQuery request, CancellationToken ct)
        {
            var stories = await _repository.GetAllAsync(ct);

            var dtoList = stories.Select(x => new SuccessStoryDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                CreatedOn = x.CreatedOn
            }).ToList();

            return Result<IReadOnlyList<SuccessStoryDto>>.Success(dtoList);
        }
    }
}
