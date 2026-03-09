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
                return Result<string>.Failure("User not found.", ErrorType.NotFound);

            var isValid = await _otpService.ValidateOtpAsync(
                request.Email, request.Otp, "PasswordReset", ct);

            if (!isValid)
                return Result<string>.Failure(
                    "Invalid or expired OTP.", ErrorType.BadRequest);

            // إعادة تعيين الباسورد
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(
                user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });
                return Result<string>.Failure(
                    "Password reset failed.", ErrorType.BadRequest, errors);
            }

            return Result<string>.Success(
                "Password has been reset successfully. You can now log in.");
        }
    }
}