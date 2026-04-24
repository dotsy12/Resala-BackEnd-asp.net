using BackEnd.Application.Common.Extensions;
using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Enums;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.EmergencyCase.Commands.UpdateEmergencyCase
{
    public class UpdateEmergencyCaseCommandHandler
      : IRequestHandler<UpdateEmergencyCaseCommand, Result<EmergencyCaseViewModel>>
    {
        private readonly IEmergencyCaseRepository _repository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<UpdateEmergencyCaseCommandHandler> _logger;

        public UpdateEmergencyCaseCommandHandler(
            IEmergencyCaseRepository repository,
            IFileUploadService fileUploadService,
            ILogger<UpdateEmergencyCaseCommandHandler> logger)
        {
            _repository = repository;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Result<EmergencyCaseViewModel>> Handle(
      UpdateEmergencyCaseCommand request,
      CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء تعديل حالة طارئة: Id={Id}", request.Id);

            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null)
            {
                return Result<EmergencyCaseViewModel>.Failure(
                    "الحالة الطارئة غير موجودة.",
                    ErrorType.NotFound);
            }

            var imageUrl = entity.ImagePath;
            var imagePublicId = entity.ImagePublicId;
            if (request.Attachment is not null)
            {
                var expectedType = request.Attachment.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                    ? UploadContentType.Document
                    : UploadContentType.Image;
                var uploadResult = await _fileUploadService.ReplaceAsync(
                    request.Attachment,
                    entity.ImagePublicId,
                    "emergency-cases",
                    expectedType,
                    cancellationToken);
                if (!uploadResult.IsSuccess)
                {
                    return Result<EmergencyCaseViewModel>.Failure(uploadResult.Message, ErrorType.BadRequest);
                }

                imageUrl = uploadResult.Value.Url;
                imagePublicId = uploadResult.Value.PublicId;
            }

            // ✅ Update basic info
            entity.UpdateDetails(request.Title, request.Description, imageUrl, imagePublicId);

            // ✅ Update urgency
            entity.SetUrgency(request.UrgencyLevel);

            // ✅ Update RequiredAmount
            if (request.RequiredAmount.HasValue)
            {
                if (request.RequiredAmount.Value <= 0)
                {
                    return Result<EmergencyCaseViewModel>.Failure(
                        "المبلغ المطلوب يجب أن يكون أكبر من صفر.",
                        ErrorType.BadRequest);
                }

                var money = new Money(request.RequiredAmount.Value, "EGP");
                entity.UpdateRequiredAmount(money);
            }

            // ✅ Activate / Deactivate
            if (request.IsActive)
                entity.Activate();
            else
                entity.Deactivate();

            // ✅ Save
            await _repository.UpdateAsync(entity, cancellationToken);

            // ✅ Mapping
            var vm = new EmergencyCaseViewModel
            {
                Image = entity.ImagePath ?? "",
                ImagePublicId = entity.ImagePublicId,
                Title = entity.Title,
                Description = entity.Description,
                TargetAmount = entity.RequiredAmount.Amount,
                ReceivedAmount = entity.CollectedAmount.Amount,
                UrgencyLevel = entity.UrgencyLevel.GetDisplayName()
            };

            _logger.LogInformation("تم تعديل الحالة الطارئة بنجاح: Id={Id}", entity.Id);

            return Result<EmergencyCaseViewModel>.Success(
                vm,
                "تم تعديل الحالة الطارئة بنجاح."
            );
        }
    }
}