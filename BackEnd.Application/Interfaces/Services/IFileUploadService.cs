using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.ResponseFormat;
using Microsoft.AspNetCore.Http;

namespace BackEnd.Application.Interfaces.Services
{
    public interface IFileUploadService
    {
        Task<Result<FileUploadResult>> UploadAsync(
            IFormFile file,
            string folder,
            UploadContentType contentType,
            CancellationToken cancellationToken = default);

        Task<Result<bool>> DeleteAsync(
            string publicId,
            CancellationToken cancellationToken = default);

        Task<Result<FileUploadResult>> ReplaceAsync(
            IFormFile file,
            string? oldPublicId,
            string folder,
            UploadContentType contentType,
            CancellationToken cancellationToken = default);
    }
}
