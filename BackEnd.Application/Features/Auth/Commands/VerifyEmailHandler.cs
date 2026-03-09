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
                return Result<string>.Failure("User not found.", ErrorType.NotFound);

            if (user.EmailConfirmed)
                return Result<string>.Failure("Email already verified.", ErrorType.BadRequest);

            var isValid = await _otpService.ValidateOtpAsync(
                request.Email, request.Otp, "EmailVerification", ct);

            if (!isValid)
                return Result<string>.Failure("Invalid or expired OTP.", ErrorType.BadRequest);

            // تفعيل الحساب
            user.EmailConfirmed = true;
            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return Result<string>.Success("Email verified successfully. You can now log in.");
        }
    }
}