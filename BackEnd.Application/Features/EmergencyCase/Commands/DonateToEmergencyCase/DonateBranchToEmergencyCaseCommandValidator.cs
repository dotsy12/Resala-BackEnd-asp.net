using FluentValidation;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public class DonateBranchToEmergencyCaseCommandValidator : AbstractValidator<DonateBranchToEmergencyCaseCommand>
    {
        public DonateBranchToEmergencyCaseCommandValidator()
        {
            RuleFor(x => x.CaseId).GreaterThan(0).WithMessage("معرف الحالة غير صحيح.");
            RuleFor(x => x.DonorId).GreaterThan(0).WithMessage("معرف المتبرع غير صحيح.");

            RuleFor(x => x.Dto.Amount)
                .GreaterThan(0).WithMessage("مبلغ التبرع يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.Dto.SlotId)
                .GreaterThan(0).WithMessage("يجب اختيار موعد متاح.");

            RuleFor(x => x.Dto.BranchContactPhone)
                .NotEmpty().WithMessage("رقم الهاتف للتواصل مطلوب.")
                .Matches(@"^01[0125]\d{8}$").WithMessage("رقم الهاتف غير صحيح.");
        }
    }
}
