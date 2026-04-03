using FluentValidation;

namespace BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase
{
    public class CreateEmergencyCaseValidator : AbstractValidator<CreateEmergencyCaseCommand>
    {
        public CreateEmergencyCaseValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("العنوان مطلوب.")
                .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("الوصف مطلوب.");

            RuleFor(x => x.RequiredAmount)
                .GreaterThan(0).WithMessage("المبلغ المطلوب يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.UrgencyLevel)
                .IsInEnum().WithMessage("مستوى الحالة غير صحيح.");
        }
    }
}