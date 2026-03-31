using FluentValidation;

namespace BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase
{
    public class CreateEmergencyCaseValidator : AbstractValidator<CreateEmergencyCaseCommand>
    {
        public CreateEmergencyCaseValidator()
        {
            RuleFor(x => x.Dto.Title)
                .NotEmpty().WithMessage("العنوان مطلوب.")
                .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

            RuleFor(x => x.Dto.Description)
                .NotEmpty().WithMessage("الوصف مطلوب.");

            RuleFor(x => x.Dto.RequiredAmount)
                .GreaterThan(0).WithMessage("المبلغ المطلوب يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.Dto.UrgencyLevel)
                .IsInEnum().WithMessage("مستوى الحالة غير صحيح.");
        }
    }
}