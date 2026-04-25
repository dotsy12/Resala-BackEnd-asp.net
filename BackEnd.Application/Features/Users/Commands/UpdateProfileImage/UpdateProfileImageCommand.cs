using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Users.Commands.UpdateProfileImage
{
    public record UpdateProfileImageCommand(string UserId, IFormFile ImageFile) : IRequest<Result<string>>;

    public class UpdateProfileImageHandler : IRequestHandler<UpdateProfileImageCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploadService _fileUploadService;

        public UpdateProfileImageHandler(
            UserManager<ApplicationUser> userManager,
            IFileUploadService fileUploadService)
        {
            _userManager = userManager;
            _fileUploadService = fileUploadService;
        }

        public async Task<Result<string>> Handle(UpdateProfileImageCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result<string>.Failure("المستخدم غير موجود.", ErrorType.NotFound);

            var uploadResult = await _fileUploadService.ReplaceAsync(
                request.ImageFile,
                user.ProfileImagePublicId,
                "profiles",
                UploadContentType.Image,
                ct);

            if (!uploadResult.IsSuccess)
                return Result<string>.Failure(uploadResult.Message, uploadResult.ErrorType);

            user.ProfileImagePath = uploadResult.Value.Url;
            user.ProfileImagePublicId = uploadResult.Value.PublicId;
            user.UpdatedOn = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result<string>.Failure("فشل تحديث صورة الملف الشخصي.", ErrorType.BadRequest);

            return Result<string>.Success(user.ProfileImagePath, "تم تحديث الصورة بنجاح.");
        }
    }
}
