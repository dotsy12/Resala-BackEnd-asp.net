using Microsoft.AspNetCore.Http;

namespace BackEnd.Application.Common.Validation
{
    public static class FileUploadValidationRules
    {
        public const long MaxFileSizeBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
        private static readonly string[] AllowedImageOrPdfExtensions = [".jpg", ".jpeg", ".png", ".pdf"];
        private static readonly string[] AllowedImageMimeTypes = ["image/jpeg", "image/png", "image/jpg"];
        private static readonly string[] AllowedImageOrPdfMimeTypes = ["image/jpeg", "image/png", "image/jpg", "application/pdf"];

        public static bool IsAllowedImage(IFormFile? file) =>
            HasAllowedContent(file, AllowedImageExtensions, AllowedImageMimeTypes);

        public static bool IsAllowedImageOrPdf(IFormFile? file) =>
            HasAllowedContent(file, AllowedImageOrPdfExtensions, AllowedImageOrPdfMimeTypes);

        public static bool IsSizeWithinLimit(IFormFile? file) =>
            file is not null && file.Length > 0 && file.Length <= MaxFileSizeBytes;

        public static bool IsSafeFileName(IFormFile? file)
        {
            if (file is null || string.IsNullOrWhiteSpace(file.FileName))
            {
                return false;
            }

            var fileName = Path.GetFileName(file.FileName);
            return fileName == file.FileName &&
                   !fileName.Contains("..", StringComparison.Ordinal) &&
                   fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        private static bool HasAllowedContent(IFormFile? file, IReadOnlyCollection<string> allowedExtensions, IReadOnlyCollection<string> allowedMimeTypes)
        {
            if (file is null || file.Length == 0)
            {
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var contentType = file.ContentType.ToLowerInvariant();

            return allowedExtensions.Contains(extension) && allowedMimeTypes.Contains(contentType);
        }
    }
}
