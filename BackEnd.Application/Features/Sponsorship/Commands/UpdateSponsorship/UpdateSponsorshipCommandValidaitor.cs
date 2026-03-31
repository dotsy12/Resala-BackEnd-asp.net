using FluentValidation;

namespace BackEnd.Application.Features.Sponsorship.Commands.UpdateSponsorship
{
    public class UpdateSponsorshipValidator : AbstractValidator<UpdateSponsorshipCommand>
    {
        public UpdateSponsorshipValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("معرف برنامج الكفالة غير صحيح.");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("اسم برنامج الكفالة مطلوب.")
                .MaximumLength(200).WithMessage("اسم برنامج الكفالة لا يتجاوز 200 حرف.");

            RuleFor(x => x.Dto.Description)
                .NotEmpty().WithMessage("الوصف مطلوب.");

            RuleFor(x => x.Dto.TargetAmount)
                .GreaterThan(0)
                .When(x => x.Dto.TargetAmount.HasValue)
                .WithMessage("المبلغ المستهدف يجب أن يكون أكبر من صفر.");
        }
    }
}