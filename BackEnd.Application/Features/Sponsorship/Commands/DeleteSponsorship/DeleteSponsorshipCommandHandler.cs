using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Sponsorship.Commands.DeleteSponsorship
{
    public class DeleteSponsorshipCommandHandler
        : IRequestHandler<DeleteSponsorshipCommand, Result<bool>>
    {
        private readonly ISponsorshipRepository _repository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<DeleteSponsorshipCommandHandler> _logger;

        public DeleteSponsorshipCommandHandler(
            ISponsorshipRepository repository,
            IFileUploadService fileUploadService,
            ILogger<DeleteSponsorshipCommandHandler> logger)
        {
            _repository = repository;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            DeleteSponsorshipCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء حذف برنامج كفالة - Id={Id}", request.Id);

            var sponsorship = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (sponsorship is null)
            {
                _logger.LogWarning("محاولة حذف برنامج كفالة غير موجود - Id={Id}", request.Id);

                return Result<bool>.Failure(
                    "برنامج الكفالة غير موجود",
                    ErrorType.NotFound
                );
            }

            if (!string.IsNullOrWhiteSpace(sponsorship.ImagePublicId))
            {
                var deleteFileResult = await _fileUploadService.DeleteAsync(
                    sponsorship.ImagePublicId,
                    cancellationToken);
                if (!deleteFileResult.IsSuccess)
                {
                    return Result<bool>.Failure(deleteFileResult.Message, deleteFileResult.ErrorType);
                }
            }

            await _repository.DeleteAsync(sponsorship, cancellationToken);

            _logger.LogInformation("تم حذف برنامج الكفالة بنجاح - Id={Id}", request.Id);

            return Result<bool>.Success(true, "تم حذف برنامج الكفالة بنجاح.");
        }
    }
}