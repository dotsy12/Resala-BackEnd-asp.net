using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.SuccessStories.Commands.DeleteSuccessStory
{
    public class DeleteSuccessStoryCommandHandler : IRequestHandler<DeleteSuccessStoryCommand, Result<bool>>
    {
        private readonly ISuccessStoryRepository _repository;
        private readonly IFileUploadService _uploadService;
        private readonly ILogger<DeleteSuccessStoryCommandHandler> _logger;

        public DeleteSuccessStoryCommandHandler(
            ISuccessStoryRepository repository,
            IFileUploadService uploadService,
            ILogger<DeleteSuccessStoryCommandHandler> logger)
        {
            _repository = repository;
            _uploadService = uploadService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteSuccessStoryCommand request, CancellationToken ct)
        {
            var story = await _repository.GetByIdAsync(request.Id, ct);
            if (story == null)
                return Result<bool>.Failure("قصة النجاح غير موجودة.", ErrorType.NotFound);

            await _uploadService.DeleteAsync(story.ImagePublicId, ct);

            _repository.Delete(story);
            await _repository.SaveChangesAsync(ct);

            _logger.LogInformation("Success story deleted: {Id}", request.Id);

            return Result<bool>.Success(true, "تم حذف قصة النجاح بنجاح.");
        }
    }
}
