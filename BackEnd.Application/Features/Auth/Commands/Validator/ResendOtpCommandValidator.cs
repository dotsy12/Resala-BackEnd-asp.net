using BackEnd.Application.Features.Auth.Commands;
using FluentValidation;

public class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.OtpType)
            .NotEmpty().WithMessage("نوع OTP مطلوب.")
            .Must(x => x == "EmailVerification" || x == "PasswordReset")
            .WithMessage("نوع OTP غير صحيح.");
    }
}