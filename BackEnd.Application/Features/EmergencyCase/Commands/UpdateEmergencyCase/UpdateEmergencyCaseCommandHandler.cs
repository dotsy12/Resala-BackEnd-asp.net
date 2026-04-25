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

            // ✅ FIX 1: استخدام GetByIdTrackedAsync لضمان التتبع (Tracking)
            var entity = await _repository.GetByIdTrackedAsync(request.Id, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("الحالة الطارئة غير موجودة: Id={Id}", request.Id);
                return Result<EmergencyCaseViewModel>.Failure(
                    "الحالة الطارئة غير موجودة.",
                    ErrorType.NotFound);
            }

            // ✅ FIX 2: منطق تحديث الصورة — الاحتفاظ بالقيم القديمة إذا لم يُرفع ملف جديد
            var imageUrl = entity.ImagePath;
            var imagePublicId = entity.ImagePublicId;

            if (request.Attachment is not null)
            {
                var expectedType = request.Attachment.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                    ? UploadContentType.Document
                    : UploadContentType.Image;

                // رفع الصورة الجديدة واستبدال القديمة في Cloudinary
                var uploadResult = await _fileUploadService.ReplaceAsync(
                    request.Attachment,
                    entity.ImagePublicId,   // ← يُحذف من Cloudinary إذا وُجد
                    "emergency-cases",
                    expectedType,
                    cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    _logger.LogWarning("فشل تحديث ملف الحالة الطارئة في Cloudinary: {Message}", uploadResult.Message);
                    return Result<EmergencyCaseViewModel>.Failure(uploadResult.Message, ErrorType.BadRequest);
                }

                imageUrl = uploadResult.Value.Url;
                imagePublicId = uploadResult.Value.PublicId;
            }

            try 
            {
                // ✅ FIX 3: تحديث البيانات الأساسية
                entity.UpdateDetails(request.Title, request.Description, imageUrl, imagePublicId);

                // ✅ تحديث مستوى الأهمية
                entity.SetUrgency(request.UrgencyLevel);

                // ✅ تحديث المبلغ المطلوب
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

                // ✅ حالة التنشيط
                if (request.IsActive)
                    entity.Activate();
                else
                    entity.Deactivate();

                // ✅ حفظ التغييرات — الـ entity tracked الآن
                await _repository.UpdateAsync(entity, cancellationToken);

                _logger.LogInformation("تم تعديل الحالة الطارئة بنجاح: Id={Id}", entity.Id);

                return Result<EmergencyCaseViewModel>.Success(
                    new EmergencyCaseViewModel
                    {
                        Id = entity.Id,
                        Image = entity.ImagePath ?? "",
                        ImagePublicId = entity.ImagePublicId,
                        Title = entity.Title,
                        Description = entity.Description,
                        TargetAmount = entity.RequiredAmount.Amount,
                        ReceivedAmount = entity.CollectedAmount.Amount,
                        UrgencyLevel = entity.UrgencyLevel.GetDisplayName()
                    },
                    "تم تعديل الحالة الطارئة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء تحديث الحالة الطارئة في قاعدة البيانات: Id={Id}", entity.Id);
                
                // إذا رفعنا ملف جديد وفشل حفظ الـ DB، قد نرغب في تنظيفه (اختياري)
                // لكننا هنا نتبع نمط الـ Sponsorship الذي يفضل البساطة
                
                return Result<EmergencyCaseViewModel>.Failure(
                    "حدث خطأ أثناء حفظ التعديلات.",
                    ErrorType.InternalServerError);
            }
        }
    }
}