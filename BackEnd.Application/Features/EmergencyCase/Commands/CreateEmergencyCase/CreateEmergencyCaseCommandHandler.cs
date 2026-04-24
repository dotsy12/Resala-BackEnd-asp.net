using BackEnd.Application.Common.Extensions;
using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Entities.EmergencyCase;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase
{
    public class CreateEmergencyCaseCommandHandler
      : IRequestHandler<CreateEmergencyCaseCommand, Result<EmergencyCaseViewModel>>
    {
        private readonly IEmergencyCaseRepository _repository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<CreateEmergencyCaseCommandHandler> _logger;

        public CreateEmergencyCaseCommandHandler(
            IEmergencyCaseRepository repository,
            IFileUploadService fileUploadService,
            ILogger<CreateEmergencyCaseCommandHandler> logger)
        {
            _repository = repository;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Result<EmergencyCaseViewModel>> Handle(
            CreateEmergencyCaseCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء إنشاء حالة طارئة جديدة");

            var requiredAmount = new Money(request.RequiredAmount);
            string? imageUrl = null;
            string? imagePublicId = null;

            // ✅ رفع الملف أولاً
            if (request.Attachment is not null)
            {
                var expectedType = request.Attachment.ContentType
                    .Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                    ? UploadContentType.Document
                    : UploadContentType.Image;

                var uploadResult = await _fileUploadService.UploadAsync(
                    request.Attachment,
                    "emergency-cases",
                    expectedType,
                    cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    return Result<EmergencyCaseViewModel>.Failure(
                        uploadResult.Message, ErrorType.BadRequest);
                }

                imageUrl = uploadResult.Value.Url;
                imagePublicId = uploadResult.Value.PublicId;
            }

            try
            {
                var entity = BackEnd.Domain.Entities.EmergencyCase.EmergencyCase.Create(
                    request.Title,
                    request.Description,
                    request.UrgencyLevel,
                    requiredAmount,
                    imageUrl,
                    imagePublicId
                );

                var created = await _repository.CreateAsync(entity, cancellationToken);

                _logger.LogInformation("تم إنشاء الحالة الطارئة بنجاح. Id={Id}", created.Id);

                return Result<EmergencyCaseViewModel>.Success(new EmergencyCaseViewModel
                {
                    Image = created.ImagePath ?? "",
                    ImagePublicId = created.ImagePublicId,
                    Title = created.Title,
                    Description = created.Description,
                    TargetAmount = created.RequiredAmount.Amount,
                    ReceivedAmount = created.CollectedAmount.Amount,
                    UrgencyLevel = created.UrgencyLevel.GetDisplayName()
                }, "تم إنشاء الحالة الطارئة بنجاح.");
            }
            catch (DomainException)
            {
                // ✅ FIX: إذا فشل DB save وكان هناك ملف مرفوع — احذفه من Cloudinary
                await CleanupOrphanFileAsync(imagePublicId, cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ غير متوقع أثناء إنشاء حالة طارئة");

                // ✅ FIX: احذف الملف المرفوع إذا فشل الحفظ في DB
                await CleanupOrphanFileAsync(imagePublicId, cancellationToken);

                return Result<EmergencyCaseViewModel>.Failure(
                    "حدث خطأ غير متوقع أثناء إنشاء الحالة الطارئة",
                    ErrorType.ServerError);
            }
        }

        /// <summary>
        /// يحذف الملف من Cloudinary في حالة فشل العملية — يمنع الـ Orphan Files
        /// </summary>
        private async Task CleanupOrphanFileAsync(
            string? publicId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return;

            try
            {
                await _fileUploadService.DeleteAsync(publicId, cancellationToken);
                _logger.LogInformation(
                    "تم حذف الملف المعلَّق من Cloudinary بنجاح: {PublicId}", publicId);
            }
            catch (Exception cleanupEx)
            {
                // لا نُوقف التنفيذ بسبب فشل الـ cleanup — فقط نُسجِّل التحذير
                _logger.LogWarning(cleanupEx,
                    "فشل حذف الملف المعلَّق من Cloudinary: {PublicId}", publicId);
            }
        }
    }
}