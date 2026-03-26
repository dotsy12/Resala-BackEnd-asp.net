using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Auth.Commands.Handler
{
    public class ResetPasswordHandler
        : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly ILogger<ResetPasswordHandler> _logger;

        public ResetPasswordHandler(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService,
            ILogger<ResetPasswordHandler> logger)
        {
            _userManager = userManager;
            _otpService = otpService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(
            ResetPasswordCommand request, CancellationToken ct)
        {
            _logger.LogInformation(
                "Reset password request started: {Email}",
                request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                _logger.LogWarning(
                    "Reset password failed — user not found: {Email}",
                    request.Email);

                return Result<string>.Failure(
                    "المستخدم غير موجود.", ErrorType.NotFound);
            }

            // Validate OTP
            var isValid = await _otpService.ValidateOtpAsync(
                request.Email, request.Otp, "PasswordReset", ct);

            if (!isValid)
            {
                _logger.LogWarning(
                    "Reset password failed — invalid or expired OTP: {Email}",
                    request.Email);

                return Result<string>.Failure(
                    "رمز OTP غير صحيح أو منتهي الصلاحية.", ErrorType.BadRequest);
            }

            _logger.LogInformation(
                "OTP validated successfully: {Email}",
                request.Email);

            // Reset password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Reset password failed — identity error: {Email}, Errors: {@Errors}",
                    request.Email,
                    result.Errors);

                var errors = result.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });

                return Result<string>.Failure(
                    "فشل إعادة تعيين الباسورد.", ErrorType.BadRequest, errors);
            }

            _logger.LogInformation(
                "Password reset successful: {UserId}",
                user.Id);

            return Result<string>.Success(
               "تم تغيير الباسورد بنجاح. يمكنك تسجيل الدخول الآن.");
        }
    }
}