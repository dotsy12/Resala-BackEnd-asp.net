using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Infrastructure.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackEnd.Infrastructure.Services
{
    public sealed class CloudinaryFileUploadService : IFileUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryFileUploadService> _logger;

        public CloudinaryFileUploadService(
            IOptions<CloudinarySettings> settings,
            ILogger<CloudinaryFileUploadService> logger)
        {
            var value = settings.Value;
            if (string.IsNullOrWhiteSpace(value.CloudName) ||
                string.IsNullOrWhiteSpace(value.ApiKey) ||
                string.IsNullOrWhiteSpace(value.ApiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing.");
            }

            var account = new Account(value.CloudName, value.ApiKey, value.ApiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
            _logger = logger;
        }

        public async Task<Result<FileUploadResult>> UploadAsync(
            IFormFile file,
            string folder,
            UploadContentType contentType,
            CancellationToken cancellationToken = default)
        {
            if (file is null || file.Length == 0)
                return Result<FileUploadResult>.Failure("الملف غير صالح.", ErrorType.BadRequest);

            try
            {
                await using var stream = file.OpenReadStream();
                var fileDescription = new FileDescription(file.FileName, stream);

                RawUploadResult uploadResult;

                if (contentType == UploadContentType.Document)
                {
                    var rawParams = new RawUploadParams
                    {
                        File = fileDescription,
                        Folder = folder,
                        UseFilename = true,
                        UniqueFilename = true,
                        Overwrite = false
                    };
                    uploadResult = await Task.Run(() => _cloudinary.Upload(rawParams), cancellationToken);
                }
                else
                {
                    var imageParams = new ImageUploadParams
                    {
                        File = fileDescription,
                        Folder = folder,
                        UseFilename = true,
                        UniqueFilename = true,
                        Overwrite = false
                    };
                    uploadResult = await _cloudinary.UploadAsync(imageParams, cancellationToken);
                }

                if (uploadResult.Error is not null || string.IsNullOrWhiteSpace(uploadResult.SecureUrl?.ToString()))
                {
                    var reason = uploadResult.Error?.Message ?? "Upload failed.";
                    _logger.LogWarning("Cloudinary upload failed: {Reason}", reason);
                    return Result<FileUploadResult>.Failure("فشل رفع الملف.", ErrorType.InternalServerError);
                }

                // ✅ نحفظ نوع الـ resource مع الـ publicId لاستخدامه عند الحذف
                // الـ PublicId يأتي من Cloudinary ويحتوي على المسار الكامل
                _logger.LogInformation(
                    "Cloudinary upload succeeded: PublicId={PublicId}, ResourceType={ResourceType}",
                    uploadResult.PublicId,
                    contentType == UploadContentType.Document ? "raw" : "image");

                return Result<FileUploadResult>.Success(new FileUploadResult(
                    uploadResult.SecureUrl.ToString(),
                    uploadResult.PublicId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while uploading to Cloudinary.");
                return Result<FileUploadResult>.Failure("تعذر رفع الملف.", ErrorType.InternalServerError);
            }
        }

        // ✅ DeleteAsync — يحاول حذف الملف بنوعيه (Image و Raw) لضمان الشمولية
        public async Task<Result<bool>> DeleteAsync(
            string publicId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return Result<bool>.Success(true);

            try
            {
                _logger.LogInformation("محاولة حذف ملف من Cloudinary: {PublicId}", publicId);

                // 1. محاولة الحذف كـ Image (الأكثر شيوعاً)
                var imageDeleteParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
                var imageResult = await _cloudinary.DestroyAsync(imageDeleteParams);

                if (imageResult.Result == "ok")
                {
                    _logger.LogInformation("تم حذف الملف بنجاح (image): {PublicId}", publicId);
                    return Result<bool>.Success(true);
                }

                // 2. إذا لم يُحذف كـ Image، نجرب كـ Raw (مثل ملفات PDF)
                _logger.LogDebug("لم يتم العثور على الملف كـ Image، نحاول كـ Raw: {PublicId}", publicId);
                var rawDeleteParams = new DeletionParams(publicId) { ResourceType = ResourceType.Raw };
                var rawResult = await _cloudinary.DestroyAsync(rawDeleteParams);

                if (rawResult.Result == "ok")
                {
                    _logger.LogInformation("تم حذف الملف بنجاح (raw): {PublicId}", publicId);
                    return Result<bool>.Success(true);
                }

                // 3. إذا كان الملف غير موجود أصلاً (Idempotent Delete)
                if (imageResult.Result == "not found" && rawResult.Result == "not found")
                {
                    _logger.LogWarning("الملف غير موجود في Cloudinary (ربما تم حذفه مسبقاً): {PublicId}", publicId);
                    return Result<bool>.Success(true);
                }

                // 4. معالجة الأخطاء الأخرى
                var error = imageResult.Error?.Message ?? rawResult.Error?.Message ?? "Unknown Cloudinary Error";
                var status = imageResult.Result != "not found" ? imageResult.Result : rawResult.Result;
                
                _logger.LogError("فشل حذف الملف من Cloudinary: {PublicId}. النتيجة: {Status}, الخطأ: {Error}", 
                    publicId, status, error);

                return Result<bool>.Failure($"فشل حذف الملف من التخزين. الحالة: {status}, الخطأ: {error}", ErrorType.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ غير متوقع أثناء حذف الملف من Cloudinary: {PublicId}", publicId);
                return Result<bool>.Failure("تعذر إتمام عملية حذف الملف من التخزين.", ErrorType.InternalServerError);
            }
        }

        // ✅ ReplaceAsync — رفع الجديد أولاً لضمان عدم فقدان البيانات
        public async Task<Result<FileUploadResult>> ReplaceAsync(
            IFormFile file,
            string? oldPublicId,
            string folder,
            UploadContentType contentType,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("بدء عملية استبدال ملف في المجلد: {Folder}", folder);

            // Step 1: رفع الملف الجديد
            var uploadResult = await UploadAsync(file, folder, contentType, cancellationToken);
            if (!uploadResult.IsSuccess)
                return uploadResult;

            // Step 2: حذف الملف القديم إذا وُجد
            if (!string.IsNullOrWhiteSpace(oldPublicId))
            {
                var deleteResult = await DeleteAsync(oldPublicId, cancellationToken);
                
                if (!deleteResult.IsSuccess)
                {
                    // ✅ ملاحظة هامة: لا نفشل العملية الكلية إذا فشل حذف الملف القديم
                    // فقط نسجل تحذير، لأن الملف الجديد تم رفعه بنجاح والبيانات في DB ستحدث
                    _logger.LogWarning(
                        "تم رفع الملف الجديد بنجاح {NewPublicId}، ولكن فشل حذف الملف القديم {OldPublicId}. سيترك كملف orphan.",
                        uploadResult.Value.PublicId, oldPublicId);
                }
            }

            return uploadResult;
        }
    }
}