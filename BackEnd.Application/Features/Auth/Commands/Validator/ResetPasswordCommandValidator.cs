using BackEnd.Application.Features.Auth.Commands;
using FluentValidation;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("رمز OTP مطلوب.")
            .Length(6).WithMessage("رمز OTP يجب أن يكون 6 أرقام.")
            .Matches(@"^\d{6}$").WithMessage("رمز OTP يجب أن يتكون من أرقام فقط.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("الباسورد الجديد مطلوب.")
            .MinimumLength(8).WithMessage("الباسورد يجب أن يكون 8 أحرف على الأقل.")
            .Matches(@"[0-9]").WithMessage("الباسورد يجب أن يحتوي على رقم واحد على الأقل.");
    }
}