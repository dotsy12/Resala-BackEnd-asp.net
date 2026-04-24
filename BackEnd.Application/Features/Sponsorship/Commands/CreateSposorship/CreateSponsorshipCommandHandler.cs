using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Common.Files;
using BackEnd.Application.Dtos.Sponsorship;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.ValueObjects;
using BackEnd.Domain.Entities.Sponsorship;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Sponsorship.Commands.Create
{
    public class CreateSponsorshipCommandHandler
       : IRequestHandler<CreateSponsorshipCommand, Result<SponsorshipViewModel>>
    {
        private readonly ISponsorshipRepository _repository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<CreateSponsorshipCommandHandler> _logger;

        public CreateSponsorshipCommandHandler(
            ISponsorshipRepository repository,
            IFileUploadService fileUploadService,
            ILogger<CreateSponsorshipCommandHandler> logger)
        {
            _repository = repository;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Result<SponsorshipViewModel>> Handle(
            CreateSponsorshipCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء إنشاء برنامج كفالة جديد");

            var dto = request.Dto;

            Money? goal = null;

            if (dto.TargetAmount.HasValue)
                goal = new Money(dto.TargetAmount.Value);

            string? imageUrl = null;
            string? imagePublicId = null;
            if (dto.ImageFile is not null)
            {
                var uploadResult = await _fileUploadService.UploadAsync(
                    dto.ImageFile,
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

            var sponsorship = BackEnd.Domain.Entities.Sponsorship.Sponsorship.Create(
                dto.Name,
                dto.Description,
                dto.Icon,
                goal
            );
            sponsorship.UpdateImages(imageUrl, imagePublicId, dto.Icon);

            var created = await _repository.CreateAsync(sponsorship, cancellationToken);

            var viewModel = new SponsorshipViewModel
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                ImageUrl = created.ImagePath ?? "",
                ImagePublicId = created.ImagePublicId,
                Icon = created.IconPath ?? "",
                TargetAmount = created.FinancialGoal?.Amount,
                CollectedAmount = created.TotalCollected.Amount,
                IsActive = created.IsActive,
                CreatedAt = created.CreatedOn
            };

            _logger.LogInformation("تم إنشاء برنامج الكفالة بنجاح: Id={Id}", created.Id);

            return Result<SponsorshipViewModel>.Success(
                viewModel,
                "تم إنشاء برنامج الكفالة بنجاح."
            );
        }
    }
}