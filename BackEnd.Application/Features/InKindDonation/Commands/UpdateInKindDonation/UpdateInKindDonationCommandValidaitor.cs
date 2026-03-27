using FluentValidation;

namespace BackEnd.Application.Features.InKindDonation.Commands.UpdateInKindDonation
{
    public class UpdateInKindDonationCommandValidaitor : AbstractValidator<UpdateInKindDonationCommand>
    {
        public UpdateInKindDonationCommandValidaitor()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف التبرع غير صحيح.");

            RuleFor(x => x.DonationTypeName)
                .NotEmpty().WithMessage("نوع التبرع مطلوب.")
                .MaximumLength(200).WithMessage("نوع التبرع لا يتجاوز 200 حرف.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("الوصف لا يتجاوز 1000 حرف.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
