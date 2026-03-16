using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Auth.Commands;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class ResendOtpHandler : IRequestHandler<ResendOtpCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ResendOtpHandler(
        UserManager<ApplicationUser> userManager,
        IOtpService otpService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<Result<string>> Handle(
        ResendOtpCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<string>.Failure("لا يوجد مستخدم بهذا البريد الإلكتروني.", ErrorType.NotFound);

        // لو EmailVerification وهو بالفعل مفعّل
        if (request.OtpType == "EmailVerification" && user.EmailConfirmed)
            return Result<string>.Failure("البريد الإلكتروني مفعّل مسبقاً.", ErrorType.BadRequest);

        var otpCode = _otpService.GenerateOtp();
        await _otpService.SaveOtpAsync(user.Email!, otpCode, request.OtpType, ct);
        await _emailService.SendOtpEmailAsync(user.Email!, otpCode, request.OtpType, ct);

        return Result<string>.Success("تم إرسال رمز OTP جديد على بريدك الإلكتروني.");
    }
}