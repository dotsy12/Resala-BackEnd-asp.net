using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public class ForgotPasswordHandler
        : IRequestHandler<ForgotPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public ForgotPasswordHandler(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _otpService = otpService;
            _emailService = emailService;
        }

        public async Task<Result<string>> Handle(
            ForgotPasswordCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Result<string>.Failure(
                    "User with this email does not exist.", ErrorType.NotFound);

            var otpCode = _otpService.GenerateOtp();
            await _otpService.SaveOtpAsync(user.Email!, otpCode, "PasswordReset", ct);
            await _emailService.SendOtpEmailAsync(user.Email!, otpCode, "PasswordReset", ct);

            return Result<string>.Success("An OTP has been sent to your email address.");
        }
    }
}