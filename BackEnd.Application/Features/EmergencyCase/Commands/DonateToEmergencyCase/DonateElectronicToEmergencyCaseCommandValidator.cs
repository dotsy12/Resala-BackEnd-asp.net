using FluentValidation;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public class DonateElectronicToEmergencyCaseCommandValidator : AbstractValidator<DonateElectronicToEmergencyCaseCommand>
    {
        public DonateElectronicToEmergencyCaseCommandValidator()
        {
            RuleFor(x => x.CaseId).GreaterThan(0).WithMessage("معرف الحالة غير صحيح.");
            RuleFor(x => x.DonorId).GreaterThan(0).WithMessage("معرف المتبرع غير صحيح.");
            
            RuleFor(x => x.Dto.Amount)
                .GreaterThan(0).WithMessage("مبلغ التبرع يجب أن يكون أكبر من صفر.");
            
            RuleFor(x => x.Dto.PaymentMethod)
                .Must(m => m == 1 || m == 2)
                .WithMessage("طريقة الدفع يجب أن تكون 1 (VodafoneCash) أو 2 (InstaPay).");

            RuleFor(x => x.Dto.ReceiptImage)
                .NotNull().WithMessage("صورة الإيصال مطلوبة.");
            
            RuleFor(x => x.Dto.SenderPhoneNumber)
                .NotEmpty().WithMessage("رقم الهاتف المحول منه مطلوب.")
                .Matches(@"^01[0125]\d{8}$").WithMessage("رقم الهاتف غير صحيح (يجب أن يكون 11 رقم).");
        }
    }
}
