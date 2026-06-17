using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SuccessStory;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.SuccessStories.Queries.GetSuccessStoryById
{
    public class GetSuccessStoryByIdQueryHandler : IRequestHandler<GetSuccessStoryByIdQuery, Result<SuccessStoryDto>>
    {
        private readonly ISuccessStoryRepository _repository;

        public GetSuccessStoryByIdQueryHandler(ISuccessStoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<SuccessStoryDto>> Handle(GetSuccessStoryByIdQuery request, CancellationToken ct)
        {
            var x = await _repository.GetByIdAsync(request.Id, ct);

            if (x == null)
                return Result<SuccessStoryDto>.Failure("قصة النجاح غير موجودة.", ErrorType.NotFound);

            return Result<SuccessStoryDto>.Success(new SuccessStoryDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                CreatedOn = x.CreatedOn
            });
        }
    }
}
