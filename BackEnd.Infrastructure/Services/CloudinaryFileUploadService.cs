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
            {
                return Result<FileUploadResult>.Failure("الملف غير صالح.", ErrorType.BadRequest);
            }

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

        public async Task<Result<bool>> DeleteAsync(
            string publicId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return Result<bool>.Success(true);
            }

            try
            {
                var deleteParams = new DeletionParams(publicId) { ResourceType = ResourceType.Auto };
                var result = await _cloudinary.DestroyAsync(deleteParams);
                if (result.Error is not null)
                {
                    _logger.LogWarning("Cloudinary delete failed for {PublicId}: {Message}",
                        publicId, result.Error.Message);
                    return Result<bool>.Failure("فشل حذف الملف من التخزين.", ErrorType.InternalServerError);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting from Cloudinary.");
                return Result<bool>.Failure("تعذر حذف الملف.", ErrorType.InternalServerError);
            }
        }

        public async Task<Result<FileUploadResult>> ReplaceAsync(
            IFormFile file,
            string? oldPublicId,
            string folder,
            UploadContentType contentType,
            CancellationToken cancellationToken = default)
        {
            var uploadResult = await UploadAsync(file, folder, contentType, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                return uploadResult;
            }

            if (!string.IsNullOrWhiteSpace(oldPublicId))
            {
                var deleteResult = await DeleteAsync(oldPublicId, cancellationToken);
                if (!deleteResult.IsSuccess)
                {
                    // rollback newly uploaded file
                    await DeleteAsync(uploadResult.Value.PublicId, cancellationToken);
                    return Result<FileUploadResult>.Failure(deleteResult.Message, deleteResult.ErrorType);
                }
            }

            return uploadResult;
        }
    }
}
