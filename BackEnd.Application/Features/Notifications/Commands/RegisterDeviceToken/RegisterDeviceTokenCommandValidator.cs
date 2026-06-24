using FluentValidation;

namespace BackEnd.Application.Features.Notifications.Commands.RegisterDeviceToken
{
    public class RegisterDeviceTokenCommandValidator : AbstractValidator<RegisterDeviceTokenCommand>
    {
        public RegisterDeviceTokenCommandValidator()
        {
            RuleFor(x => x.DonorId)
                .GreaterThan(0).WithMessage("معرف المتبرع غير صالح (يجب أن يكون أكبر من 0).");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("رمز الجهاز (Token) مطلوب ولا يمكن أن يكون فارغاً.")
                .MinimumLength(20).WithMessage("رمز الجهاز غير صالح (يجب ألا يقل عن 20 حرفاً).");

            RuleFor(x => x.DeviceType)
                .MaximumLength(50).WithMessage("نوع الجهاز طويل جداً (الحد الأقصى 50 حرفاً).");
        }
    }
}
