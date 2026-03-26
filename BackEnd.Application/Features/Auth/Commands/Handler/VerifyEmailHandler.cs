using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Auth.Commands.Handler
{
    public class VerifyEmailHandler
        : IRequestHandler<VerifyEmailCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly ILogger<VerifyEmailHandler> _logger;

        public VerifyEmailHandler(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService,
            ILogger<VerifyEmailHandler> logger)
        {
            _userManager = userManager;
            _otpService = otpService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(
            VerifyEmailCommand request, CancellationToken ct)
        {
            _logger.LogInformation(
                "Email verification started: {Email}",
                request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                _logger.LogWarning(
                    "Email verification failed — user not found: {Email}",
                    request.Email);

                return Result<string>.Failure(
                    "المستخدم غير موجود.", ErrorType.NotFound);
            }

            if (user.EmailConfirmed)
            {
                _logger.LogWarning(
                    "Email verification blocked — already confirmed: {Email}",
                    request.Email);

                return Result<string>.Failure(
                    "البريد الإلكتروني مفعّل مسبقاً.", ErrorType.BadRequest);
            }

            // Validate OTP
            var isValid = await _otpService.ValidateOtpAsync(
                request.Email, request.Otp, "EmailVerification", ct);

            if (!isValid)
            {
                _logger.LogWarning(
                    "Email verification failed — invalid or expired OTP: {Email}",
                    request.Email);

                return Result<string>.Failure(
                    "رمز OTP غير صحيح أو منتهي الصلاحية.", ErrorType.BadRequest);
            }

            _logger.LogInformation(
                "OTP validated successfully for email verification: {Email}",
                request.Email);

            // Activate user
            user.EmailConfirmed = true;
            user.IsActive = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Email verification failed — update user error: {Email}, Errors: {@Errors}",
                    request.Email,
                    result.Errors);

                return Result<string>.Failure(
                    "حدث خطأ أثناء تفعيل الحساب.", ErrorType.BadRequest);
            }

            _logger.LogInformation(
                "Email verified successfully: {UserId}",
                user.Id);

            return Result<string>.Success(
                "تم التحقق من البريد الإلكتروني بنجاح. يمكنك تسجيل الدخول الآن.");
        }
    }
}