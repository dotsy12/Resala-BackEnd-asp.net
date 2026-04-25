using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Users.Commands.UpdateProfile
{
    public record UpdateProfileCommand(string UserId, UpdateProfileDto Dto) : IRequest<Result<bool>>;

    public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateProfileHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<bool>> Handle(UpdateProfileCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result<bool>.Failure("المستخدم غير موجود.", ErrorType.NotFound);

            var names = request.Dto.FullName.Split(' ', 2);
            user.FirstName = names[0];
            user.LastName = names.Length > 1 ? names[1] : "";
            user.PhoneNumber = request.Dto.Phone;
            user.Address = request.Dto.Address;
            user.Governorate = request.Dto.Governorate;
            user.UpdatedOn = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result<bool>.Failure("فشل تحديث البيانات.", ErrorType.BadRequest);
            }

            return Result<bool>.Success(true, "تم تحديث الملف الشخصي بنجاح.");
        }
    }
}
