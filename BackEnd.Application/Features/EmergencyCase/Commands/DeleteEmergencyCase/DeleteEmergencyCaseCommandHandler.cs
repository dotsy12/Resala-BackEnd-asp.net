using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DeleteEmergencyCase
{
    public class DeleteEmergencyCaseCommandHandler
        : IRequestHandler<DeleteEmergencyCaseCommand, Result<bool>>
    {
        private readonly IEmergencyCaseRepository _repository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<DeleteEmergencyCaseCommandHandler> _logger;

        public DeleteEmergencyCaseCommandHandler(
            IEmergencyCaseRepository repository,
            IFileUploadService fileUploadService,
            ILogger<DeleteEmergencyCaseCommandHandler> logger)
        {
            _repository = repository;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            DeleteEmergencyCaseCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء حذف حالة طارئة: Id={Id}", request.id);

            // ✅ FIX 1: استخدام GetByIdTrackedAsync لضمان التتبع (Tracking)
            var entity = await _repository.GetByIdTrackedAsync(request.id, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("محاولة حذف حالة طارئة غير موجودة: Id={Id}", request.id);
                return Result<bool>.Failure(
                    "الحالة الطارئة غير موجودة.",
                    ErrorType.NotFound);
            }

            // ✅ التحقق من وجود تبرعات — منع الحذف إذا وُجدت
            if (entity.CollectedAmount.Amount > 0)
            {
                _logger.LogWarning("فشل حذف الحالة Id={Id} لأنها تحتوي على تبرعات.", request.id);
                return Result<bool>.Failure(
                    "لا يمكن حذف حالة لديها تبرعات بالفعل.",
                    ErrorType.Conflict);
            }

            // ✅ FIX 2: حذف الملف من Cloudinary
            if (!string.IsNullOrWhiteSpace(entity.ImagePublicId))
            {
                var deleteFileResult = await _fileUploadService.DeleteAsync(
                    entity.ImagePublicId,
                    cancellationToken);

                if (!deleteFileResult.IsSuccess)
                {
                    // ✅ نسجل الخطأ بالكامل ولكن لا نوقف حذف السجل من قاعدة البيانات
                    // هذا يمنع بقاء سجلات "عالمة" بسبب مشاكل في Storage
                    _logger.LogError(
                        "فشل حذف الملف من Cloudinary للحالة الطارئة Id={Id}, PublicId={PublicId}: {Message}",
                        entity.Id,
                        entity.ImagePublicId,
                        deleteFileResult.Message);
                }
                else
                {
                    _logger.LogInformation(
                        "تم حذف الملف من Cloudinary بنجاح: PublicId={PublicId}",
                        entity.ImagePublicId);
                }
            }

            // ✅ FIX 3: حذف حقيقي من قاعدة البيانات لمطابقة الـ Sponsorship
            // أو يمكن استخدام entity.IsDeleted = true إذا كان الـ repository يدعم Soft Delete
            await _repository.DeleteAsync(entity, cancellationToken);

            _logger.LogInformation("تم حذف الحالة الطارئة بنجاح من قاعدة البيانات والتخزين: Id={Id}", request.id);

            return Result<bool>.Success(true, "تم حذف الحالة الطارئة بنجاح.");
        }
    }
}