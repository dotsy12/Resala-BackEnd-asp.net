using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.Validation;
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

            // ✅ FIX 1: GetByIdTrackedAsync — entity tracked في EF Core context
            var sponsorship = await _repository.GetByIdTrackedAsync(request.Id, cancellationToken);

            if (sponsorship is null)
                return Result<SponsorshipViewModel>.Failure(
                    "برنامج الكفالة غير موجود.", ErrorType.NotFound);

            var dto = request.Dto;

            // ✅ FIX 2: Image Update Logic — الاحتفاظ بالقيم القديمة إذا لم يُرفع ملف جديد
            string? imageUrl = sponsorship.ImagePath;
            string? imagePublicId = sponsorship.ImagePublicId;

            if (dto.ImageFile is not null)
            {
                // رفع الصورة الجديدة واستبدال القديمة في Cloudinary
                var uploadResult = await _fileUploadService.ReplaceAsync(
                    dto.ImageFile,
                    sponsorship.ImagePublicId,   // ← يُحذف من Cloudinary إذا وُجد
                    "sponsorships",
                    UploadContentType.Image,
                    cancellationToken);

                if (!uploadResult.IsSuccess)
                    return Result<SponsorshipViewModel>.Failure(
                        uploadResult.Message, ErrorType.BadRequest);

                imageUrl = uploadResult.Value.Url;
                imagePublicId = uploadResult.Value.PublicId;
            }

            // ✅ FIX 3: Icon Update Logic — منطق واضح بدون تناقضات
            string? iconUrl = sponsorship.IconPath;       // احتفظ بالقديم افتراضياً
            string? iconPublicId = sponsorship.IconPublicId;

            if (dto.IconFile is not null)
            {
                // رفع Icon جديد
                var iconUploadResult = await _fileUploadService.ReplaceAsync(
                    dto.IconFile,
                    sponsorship.IconPublicId,    // ← يُحذف من Cloudinary إذا وُجد
                    "sponsorships/icons",
                    UploadContentType.Image,
                    cancellationToken);

                if (!iconUploadResult.IsSuccess)
                    return Result<SponsorshipViewModel>.Failure(
                        iconUploadResult.Message, ErrorType.BadRequest);

                iconUrl = iconUploadResult.Value.Url;
                iconPublicId = iconUploadResult.Value.PublicId;
            }
            else if (!string.IsNullOrWhiteSpace(dto.Icon))
            {
                // Icon كـ string نصي (اسم أيقونة CSS/FontAwesome)
                iconUrl = dto.Icon;
                // iconPublicId يبقى كما هو — لم يُستبدل بملف
            }
            // else: لم يُرسل Icon جديد → نبقي الموجود

            // ✅ FIX 4: تحديث البيانات الأساسية (Name, Description, TargetAmount)
            var financialGoal = dto.TargetAmount.HasValue 
                ? new Money(dto.TargetAmount.Value) 
                : null;
            
            sponsorship.UpdateDetails(dto.Name, dto.Description, financialGoal);

            // ✅ FIX 5: تحديث الصور والـ Policy والحالة
            sponsorship.UpdateImages(imageUrl, imagePublicId, iconUrl, iconPublicId);
            sponsorship.UpdatePolicy(sponsorship.Policy);

            if (dto.IsActive)
                sponsorship.Activate();
            else
                sponsorship.Deactivate();

            // ✅ FIX 6: UpdateAsync على tracked entity — EF Core يرصد التغييرات
            await _repository.UpdateAsync(sponsorship, cancellationToken);

            _logger.LogInformation("تم تعديل برنامج الكفالة بنجاح: Id={Id}", sponsorship.Id);

            return Result<SponsorshipViewModel>.Success(
                new SponsorshipViewModel
                {
                    Id = sponsorship.Id,
                    Name = sponsorship.Name,
                    Description = sponsorship.Description,
                    ImageUrl = sponsorship.ImagePath ?? "",
                    ImagePublicId = sponsorship.ImagePublicId,
                    Icon = sponsorship.IconPath ?? "",
                    IconPublicId = sponsorship.IconPublicId,
                    TargetAmount = sponsorship.FinancialGoal?.Amount,
                    CollectedAmount = sponsorship.TotalCollected.Amount,
                    IsActive = sponsorship.IsActive,
                    CreatedAt = sponsorship.CreatedOn
                },
                "تم تعديل برنامج الكفالة بنجاح."
            );
        }
    }
}
