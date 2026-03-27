using BackEnd.Application.Features.InKindDonation.Commands.CreateInKindDonation;
using FluentValidation;


namespace BackEnd.Application.Features.InKindDonation.Commands.RecordInKindDonation
{
    public class RecordInKindDonationCommandValidaitor : AbstractValidator<RecordInKindDonationCommand>
    {
        public RecordInKindDonationCommandValidaitor()
        {
            RuleFor(x => x.DonorId)
            .GreaterThan(0).WithMessage("يجب اختيار متبرع صحيح.");

            RuleFor(x => x.DonationTypeName)
                .NotEmpty().WithMessage("نوع التبرع مطلوب.")
                .MaximumLength(200).WithMessage("نوع التبرع لا يتجاوز 200 حرف.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("الوصف لا يتجاوز 1000 حرف.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.RecordedByStaffId)
                .GreaterThan(0).WithMessage("معرف الموظف غير صحيح.");
        }


    }
}
