using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public class VerifyEmailHandler
        : IRequestHandler<VerifyEmailCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;

        public VerifyEmailHandler(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService)
        {
            _userManager = userManager;
            _otpService = otpService;
        }

        public async Task<Result<string>> Handle(
            VerifyEmailCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Result<string>.Failure("المستخدم غير موجود.", ErrorType.NotFound);

            if (user.EmailConfirmed)
                return Result<string>.Failure("البريد الإلكتروني مفعّل مسبقاً.", ErrorType.BadRequest);

            var isValid = await _otpService.ValidateOtpAsync(
                request.Email, request.Otp, "EmailVerification", ct);

            if (!isValid)
                return Result<string>.Failure("رمز OTP غير صحيح أو منتهي الصلاحية.", ErrorType.BadRequest);

            // تفعيل الحساب
            user.EmailConfirmed = true;
            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return Result<string>.Success("تم التحقق من البريد الإلكتروني بنجاح. يمكنك تسجيل الدخول الآن.");
        }
    }
}