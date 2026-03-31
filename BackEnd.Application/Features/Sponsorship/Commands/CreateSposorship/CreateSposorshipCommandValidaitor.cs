using BackEnd.Application.Features.Sponsorship.Commands.Create;
using FluentValidation;

namespace BackEnd.Application.Features.Sponsorship.Commands.CreateSposorship
{
    public class CreateSponsorshipCommandValidator : AbstractValidator<CreateSponsorshipCommand>
    {
        public CreateSponsorshipCommandValidator()
        {
            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("اسم برنامج الكفالة مطلوب.");

            RuleFor(x => x.Dto.Description)
                .NotEmpty().WithMessage("الوصف مطلوب.");

            RuleFor(x => x.Dto.TargetAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("المبلغ المستهدف يجب أن يكون صفر أو أكبر.");
        }
    }
}