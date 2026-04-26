using FluentValidation;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public class DonateRepresentativeToEmergencyCaseCommandValidator : AbstractValidator<DonateRepresentativeToEmergencyCaseCommand>
    {
        public DonateRepresentativeToEmergencyCaseCommandValidator()
        {
            RuleFor(x => x.CaseId).GreaterThan(0).WithMessage("معرف الحالة غير صحيح.");
            RuleFor(x => x.DonorId).GreaterThan(0).WithMessage("معرف المتبرع غير صحيح.");

            RuleFor(x => x.Dto.Amount)
                .GreaterThan(0).WithMessage("مبلغ التبرع يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.Dto.DeliveryAreaId)
                .GreaterThan(0).WithMessage("منطقة التوصيل مطلوبة.");

            RuleFor(x => x.Dto.Address)
                .NotEmpty().WithMessage("العنوان مطلوب.");

            RuleFor(x => x.Dto.ContactName)
                .NotEmpty().WithMessage("اسم الشخص للتواصل مطلوب.");

            RuleFor(x => x.Dto.ContactPhone)
                .NotEmpty().WithMessage("رقم هاتف التواصل مطلوب.")
                .Matches(@"^01[0125]\d{8}$").WithMessage("رقم الهاتف غير صحيح.");
        }
    }
}
