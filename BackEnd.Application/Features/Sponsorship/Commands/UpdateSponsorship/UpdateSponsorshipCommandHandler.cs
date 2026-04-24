using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Common.Files;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Sponsorship.Commands.UpdateSponsorship
{
    public class UpdateSponsorshipCommandHandler
     : IRequestHandler<UpdateSponsorshipCommand, Result<SponsorshipViewModel>>
    {
        private readonly ISponsorshipRepository _repository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<UpdateSponsorshipCommandHandler> _logger;

        public UpdateSponsorshipCommandHandler(
            ISponsorshipRepository repository,
            IFileUploadService fileUploadService,
            ILogger<UpdateSponsorshipCommandHandler> logger)
        {
            _repository = repository;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Result<SponsorshipViewModel>> Handle(
            UpdateSponsorshipCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء تعديل برنامج كفالة: Id={Id}", request.Id);

            var sponsorship = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (sponsorship is null)
            {
                return Result<SponsorshipViewModel>.Failure(
                    "برنامج الكفالة غير موجود.",
                    ErrorType.NotFound
                );
            }

            var dto = request.Dto;

            string? imageUrl = sponsorship.ImagePath;
            string? imagePublicId = sponsorship.ImagePublicId;
            if (dto.ImageFile is not null)
            {
                var uploadResult = await _fileUploadService.ReplaceAsync(
                    dto.ImageFile,
                    sponsorship.ImagePublicId,
                    "sponsorships",
                    UploadContentType.Image,
                    cancellationToken);
                if (!uploadResult.IsSuccess)
                {
                    return Result<SponsorshipViewModel>.Failure(uploadResult.Message, ErrorType.BadRequest);
                }

                imageUrl = uploadResult.Value.Url;
                imagePublicId = uploadResult.Value.PublicId;
            }

            // ✅ Update images
            sponsorship.UpdateImages(imageUrl, imagePublicId, dto.Icon);

           
             sponsorship.UpdatePolicy(sponsorship.Policy);

            // ✅ Activate / Deactivate
            if (dto.IsActive)
                sponsorship.Activate();
            else
                sponsorship.Deactivate();

            await _repository.UpdateAsync(sponsorship, cancellationToken);

            var viewModel = new SponsorshipViewModel
            {
                Id = sponsorship.Id,
                Name = sponsorship.Name,
                Description = sponsorship.Description,
                ImageUrl = sponsorship.ImagePath ?? "",
                ImagePublicId = sponsorship.ImagePublicId,
                Icon = sponsorship.IconPath ?? "",
                TargetAmount = sponsorship.FinancialGoal?.Amount,
                CollectedAmount = sponsorship.TotalCollected.Amount,
                IsActive = sponsorship.IsActive,
                CreatedAt = sponsorship.CreatedOn
            };

            _logger.LogInformation("تم تعديل برنامج الكفالة بنجاح: Id={Id}", sponsorship.Id);

            return Result<SponsorshipViewModel>.Success(
                viewModel,
                "تم تعديل برنامج الكفالة بنجاح."
            );
        }
    }
}