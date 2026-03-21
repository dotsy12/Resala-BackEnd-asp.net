using FluentValidation;

namespace BackEnd.Application.Features.Auth.Commands
{
    public class RegisterDonorCommandValidator : AbstractValidator<RegisterDonorCommand>
    {
        public RegisterDonorCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("الاسم مطلوب.")
                .MinimumLength(3).WithMessage("الاسم يجب أن يكون 3 أحرف على الأقل.")
                .MaximumLength(100).WithMessage("الاسم لا يتجاوز 100 حرف.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
                .Matches(@"^01[0-2,5]{1}[0-9]{8}$")
                .WithMessage("رقم الهاتف يجب أن يكون رقم مصري صحيح (مثال: 01012345678).");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("الباسورد مطلوب.")
                .MinimumLength(8).WithMessage("الباسورد يجب أن يكون 8 أحرف على الأقل.")
                .Matches(@"[0-9]").WithMessage("الباسورد يجب أن يحتوي على رقم واحد على الأقل.");

            RuleFor(x => x.Job)
                .MaximumLength(100).WithMessage("المهنة لا تتجاوز 100 حرف.")
                .When(x => !string.IsNullOrEmpty(x.Job));
        }
    }
}