using BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase;
using BackEnd.Domain.Enums;
using FluentValidation;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public class DonateToEmergencyCaseCommandValidator : AbstractValidator<DonateToEmergencyCaseCommand>
    {
        public DonateToEmergencyCaseCommandValidator()
        {
            RuleFor(x => x.CaseId).GreaterThan(0).WithMessage("معرف الحالة غير صحيح.");
            RuleFor(x => x.DonorId).GreaterThan(0).WithMessage("معرف المتبرع غير صحيح.");
            RuleFor(x => x.Dto.Amount).GreaterThan(0).WithMessage("مبلغ التبرع يجب أن يكون أكبر من صفر.");
            RuleFor(x => x.Dto.PaymentMethod).IsInEnum().WithMessage("طريقة الدفع غير صالحة.");

            // ── VodafoneCash / InstaPay Validation ──────────────────
            When(x => x.Dto.PaymentMethod == PaymentMethod.VodafoneCash || x.Dto.PaymentMethod == PaymentMethod.InstaPay, () =>
            {
                RuleFor(x => x.Dto.ReceiptImage)
                    .NotNull().WithMessage("صورة الإيصال مطلوبة لهذا النوع من الدفع.");
                
                RuleFor(x => x.Dto.SenderPhoneNumber)
                    .NotEmpty().WithMessage("رقم الهاتف المحول منه مطلوب.")
                    .Matches(@"^01[0125]\d{8}$").WithMessage("رقم الهاتف غير صحيح.");
            });

            // ── Representative Validation ───────────────────────────
            When(x => x.Dto.PaymentMethod == PaymentMethod.Representative, () =>
            {
                RuleFor(x => x.Dto.DeliveryAreaId)
                    .NotNull().WithMessage("منطقة التوصيل مطلوبة.");
                
                RuleFor(x => x.Dto.Address)
                    .NotEmpty().WithMessage("العنوان مطلوب.");
                
                RuleFor(x => x.Dto.ContactName)
                    .NotEmpty().WithMessage("اسم الشخص للتواصل مطلوب.");
                
                RuleFor(x => x.Dto.ContactPhone)
                    .NotEmpty().WithMessage("رقم هاتف التواصل مطلوب.")
                    .Matches(@"^01[0125]\d{8}$").WithMessage("رقم الهاتف غير صحيح.");
            });

            // ── Branch Validation ──────────────────────────────────
            When(x => x.Dto.PaymentMethod == PaymentMethod.Branch, () =>
            {
                RuleFor(x => x.Dto.SlotId)
                    .NotNull().WithMessage("يجب اختيار موعد متاح.");
                
                RuleFor(x => x.Dto.BranchContactPhone)
                    .NotEmpty().WithMessage("رقم الهاتف للتواصل مطلوب.")
                    .Matches(@"^01[0125]\d{8}$").WithMessage("رقم الهاتف غير صحيح.");
            });
        }
    }
}
