using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Auth.Commands;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

public class ResendOtpHandler : IRequestHandler<ResendOtpCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResendOtpHandler> _logger;

    public ResendOtpHandler(
        UserManager<ApplicationUser> userManager,
        IOtpService otpService,
        IEmailService emailService,
        ILogger<ResendOtpHandler> logger)
    {
        _userManager = userManager;
        _otpService = otpService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(
        ResendOtpCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Resend OTP request started: {Email}, Type: {OtpType}",
            request.Email, request.OtpType);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            _logger.LogWarning(
                "Resend OTP failed — user not found: {Email}",
                request.Email);

            return Result<string>.Failure(
                "لا يوجد مستخدم بهذا البريد الإلكتروني.", ErrorType.NotFound);
        }

        // Email already confirmed
        if (request.OtpType == "EmailVerification" && user.EmailConfirmed)
        {
            _logger.LogWarning(
                "Resend OTP blocked — email already confirmed: {Email}",
                request.Email);

            return Result<string>.Failure(
                "البريد الإلكتروني مفعّل مسبقاً.", ErrorType.BadRequest);
        }

        // Generate OTP
        var otpCode = _otpService.GenerateOtp();

        _logger.LogInformation(
            "OTP generated for resend: {Email}, Type: {OtpType}",
            user.Email, request.OtpType);

        // Save OTP
        await _otpService.SaveOtpAsync(user.Email!, otpCode, request.OtpType, ct);

        _logger.LogInformation(
            "OTP saved successfully: {Email}",
            user.Email);

        // Send Email
        await _emailService.SendOtpEmailAsync(user.Email!, otpCode, request.OtpType, ct);

        _logger.LogInformation(
            "OTP email sent successfully: {Email}",
            user.Email);

        return Result<string>.Success(
            "تم إرسال رمز OTP جديد على بريدك الإلكتروني.");
    }
}