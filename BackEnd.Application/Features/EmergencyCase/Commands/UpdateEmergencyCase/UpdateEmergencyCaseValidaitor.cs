using FluentValidation;

namespace BackEnd.Application.Features.EmergencyCase.Commands.UpdateEmergencyCase
{
    public class UpdateEmergencyCaseValidator : AbstractValidator<UpdateEmergencyCaseCommand>
    {
        public UpdateEmergencyCaseValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("معرف الحالة غير صحيح.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("العنوان مطلوب.")
                .MaximumLength(200)
                .WithMessage("العنوان لا يتجاوز 200 حرف.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("الوصف مطلوب.");

            RuleFor(x => x.UrgencyLevel)
                .IsInEnum()
                .WithMessage("مستوى الحالة غير صحيح.");

            RuleFor(x => x.RequiredAmount)
                .GreaterThan(0)
                .When(x => x.RequiredAmount.HasValue)
                .WithMessage("المبلغ المطلوب يجب أن يكون أكبر من صفر.");
        }
    }
}