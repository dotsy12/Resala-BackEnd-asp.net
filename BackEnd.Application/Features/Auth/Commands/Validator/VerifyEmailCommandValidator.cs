using BackEnd.Application.Features.Auth.Commands;
using FluentValidation;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("رمز OTP مطلوب.")
            .Length(6).WithMessage("رمز OTP يجب أن يكون 6 أرقام.")
            .Matches(@"^\d{6}$").WithMessage("رمز OTP يجب أن يتكون من أرقام فقط.");
    }
}