using BackEnd.Application.Features.Auth.Commands;
using FluentValidation;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)
                    || !string.IsNullOrWhiteSpace(x.Username))
            .WithMessage("رقم الهاتف أو اسم المستخدم مطلوب.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("الباسورد مطلوب.");
    }
}