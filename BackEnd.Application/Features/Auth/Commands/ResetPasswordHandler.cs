using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public class ResetPasswordHandler
        : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;

        public ResetPasswordHandler(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService)
        {
            _userManager = userManager;
            _otpService = otpService;
        }

        public async Task<Result<string>> Handle(
            ResetPasswordCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Result<string>.Failure("المستخدم غير موجود.", ErrorType.NotFound);

            var isValid = await _otpService.ValidateOtpAsync(
                request.Email, request.Otp, "PasswordReset", ct);

            if (!isValid)
                return Result<string>.Failure(
                    "رمز OTP غير صحيح أو منتهي الصلاحية.", ErrorType.BadRequest);

            // إعادة تعيين الباسورد
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(
                user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });
                return Result<string>.Failure(
                    "فشل إعادة تعيين الباسورد.", ErrorType.BadRequest, errors);
            }

            return Result<string>.Success(
               "تم تغيير الباسورد بنجاح. يمكنك تسجيل الدخول الآن.");
        }
    }
}