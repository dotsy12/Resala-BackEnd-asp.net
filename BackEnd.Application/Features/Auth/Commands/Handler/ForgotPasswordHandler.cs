using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Auth.Commands.Handler
{
    public class ForgotPasswordHandler
        : IRequestHandler<ForgotPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordHandler> _logger;

        public ForgotPasswordHandler(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService,
            IEmailService emailService,
            ILogger<ForgotPasswordHandler> logger)
        {
            _userManager = userManager;
            _otpService = otpService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(
            ForgotPasswordCommand request, CancellationToken ct)
        {
            _logger.LogInformation(
                "Forgot password request started for email: {Email}",
                request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                _logger.LogWarning(
                    "Forgot password failed — user not found: {Email}",
                    request.Email);

                return Result<string>.Failure(
                    "لا يوجد مستخدم بهذا البريد الإلكتروني.", ErrorType.NotFound);
            }

            // Generate OTP
            var otpCode = _otpService.GenerateOtp();

            _logger.LogInformation(
                "OTP generated for email: {Email}",
                user.Email);

            // Save OTP
            await _otpService.SaveOtpAsync(user.Email!, otpCode, "PasswordReset", ct);

            _logger.LogInformation(
                "OTP saved for email: {Email}",
                user.Email);

            // Send Email
            await _emailService.SendOtpEmailAsync(user.Email!, otpCode, "PasswordReset", ct);

            _logger.LogInformation(
                "OTP email sent successfully to: {Email}",
                user.Email);

            return Result<string>.Success("تم إرسال رمز OTP على بريدك الإلكتروني.");
        }
    }
}