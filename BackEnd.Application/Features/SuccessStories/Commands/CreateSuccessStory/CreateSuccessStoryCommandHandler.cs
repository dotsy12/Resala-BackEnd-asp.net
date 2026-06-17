using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SuccessStory;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.SuccessStories.Commands.CreateSuccessStory
{
    public class CreateSuccessStoryCommandHandler : IRequestHandler<CreateSuccessStoryCommand, Result<SuccessStoryDto>>
    {
        private readonly ISuccessStoryRepository _repository;
        private readonly IFileUploadService _uploadService;
        private readonly ILogger<CreateSuccessStoryCommandHandler> _logger;

        public CreateSuccessStoryCommandHandler(
            ISuccessStoryRepository repository,
            IFileUploadService uploadService,
            ILogger<CreateSuccessStoryCommandHandler> logger)
        {
            _repository = repository;
            _uploadService = uploadService;
            _logger = logger;
        }

        public async Task<Result<SuccessStoryDto>> Handle(CreateSuccessStoryCommand request, CancellationToken ct)
        {
            var uploadResult = await _uploadService.UploadAsync(
                request.Image, 
                "success-stories", 
                UploadContentType.Image, 
                ct);

            if (!uploadResult.IsSuccess)
                return Result<SuccessStoryDto>.Failure("فشل رفع الصورة.", ErrorType.BadRequest);

            var story = SuccessStory.Create(
                request.Title,
                request.Description,
                uploadResult.Value.Url,
                uploadResult.Value.PublicId);

            await _repository.AddAsync(story, ct);
            await _repository.SaveChangesAsync(ct);

            _logger.LogInformation("Success story created: {Title} (Id: {Id})", story.Title, story.Id);

            return Result<SuccessStoryDto>.Success(new SuccessStoryDto
            {
                Id = story.Id,
                Title = story.Title,
                Description = story.Description,
                ImageUrl = story.ImageUrl,
                CreatedOn = story.CreatedOn
            });
        }
    }
}
