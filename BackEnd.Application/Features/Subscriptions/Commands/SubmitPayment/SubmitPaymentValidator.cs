// SubmitPaymentValidator.cs
using BackEnd.Application.Features.Subscriptions.Commands.SubmitPayment;
using BackEnd.Application.Common.Validation;
using BackEnd.Domain.Enums;
using FluentValidation;

namespace BackEnd.Application.Features.Subscriptions.Commands.SubmitPayment
{
    public class SubmitPaymentValidator : AbstractValidator<SubmitPaymentCommand>
    {
        public SubmitPaymentValidator()
        {
            RuleFor(x => x.SubscriptionId).GreaterThan(0);
            RuleFor(x => x.DonorId).GreaterThan(0);

            RuleFor(x => x.Dto.Amount)
                .GreaterThan(0).WithMessage("المبلغ يجب أن يكون أكبر من صفر.");

            // VodafoneCash / InstaPay
            RuleFor(x => x.Dto.ReceiptImage)
                .NotNull().WithMessage("صورة إثبات الدفع مطلوبة.")
                .When(x => x.Dto.PaymentMethod is PaymentMethod.VodafoneCash
                                               or PaymentMethod.InstaPay);

            RuleFor(x => x.Dto.ReceiptImage)
                .Must(file => file is null || FileUploadValidationRules.IsAllowedImage(file))
                .WithMessage("صيغة الإيصال غير مدعومة. المسموح: jpg, jpeg, png.")
                .When(x => x.Dto.PaymentMethod is PaymentMethod.VodafoneCash
                                               or PaymentMethod.InstaPay);

            RuleFor(x => x.Dto.ReceiptImage)
                .Must(file => file is null || FileUploadValidationRules.IsSizeWithinLimit(file))
                .WithMessage("حجم الإيصال يجب ألا يتجاوز 5MB.")
                .When(x => x.Dto.PaymentMethod is PaymentMethod.VodafoneCash
                                               or PaymentMethod.InstaPay);

            RuleFor(x => x.Dto.ReceiptImage)
                .Must(file => file is null || FileUploadValidationRules.IsSafeFileName(file))
                .WithMessage("اسم ملف الإيصال غير آمن.")
                .When(x => x.Dto.PaymentMethod is PaymentMethod.VodafoneCash
                                               or PaymentMethod.InstaPay);

            RuleFor(x => x.Dto.SenderPhoneNumber)
                .NotEmpty().WithMessage("رقم الهاتف المُحوَّل منه مطلوب.")
                .Matches(@"^01[0-2,5]{1}[0-9]{8}$")
                .WithMessage("رقم الهاتف يجب أن يكون رقم مصري صحيح.")
                .When(x => x.Dto.PaymentMethod is PaymentMethod.VodafoneCash
                                               or PaymentMethod.InstaPay);

            // Representative
            RuleFor(x => x.Dto.DeliveryAreaId)
                .NotNull().WithMessage("منطقة التوصيل مطلوبة.")
                .GreaterThan(0)
                .When(x => x.Dto.PaymentMethod == PaymentMethod.Representative);

            RuleFor(x => x.Dto.ContactName)
                .NotEmpty().WithMessage("اسم جهة الاتصال مطلوب.")
                .MaximumLength(100)
                .When(x => x.Dto.PaymentMethod == PaymentMethod.Representative);

            RuleFor(x => x.Dto.ContactPhone)
                .NotEmpty().WithMessage("رقم جهة الاتصال مطلوب.")
                .Matches(@"^01[0-2,5]{1}[0-9]{8}$")
                .WithMessage("رقم الهاتف يجب أن يكون رقم مصري صحيح.")
                .When(x => x.Dto.PaymentMethod == PaymentMethod.Representative);

            RuleFor(x => x.Dto.Address)
                .NotEmpty().WithMessage("العنوان التفصيلي مطلوب.")
                .MaximumLength(500)
                .When(x => x.Dto.PaymentMethod == PaymentMethod.Representative);

            // Branch
            RuleFor(x => x.Dto.SlotId)
                .NotNull().WithMessage("يجب اختيار موعد.")
                .GreaterThan(0)
                .When(x => x.Dto.PaymentMethod == PaymentMethod.Branch);

            RuleFor(x => x.Dto.DonorName)
                .NotEmpty().WithMessage("اسم المتبرع مطلوب للزيارة.")
                .MaximumLength(100)
                .When(x => x.Dto.PaymentMethod == PaymentMethod.Branch);

            RuleFor(x => x.Dto.BranchContactPhone)
                .NotEmpty().WithMessage("رقم الهاتف للتواصل مطلوب.")
                .Matches(@"^01[0-2,5]{1}[0-9]{8}$")
                .When(x => x.Dto.PaymentMethod == PaymentMethod.Branch);

            RuleFor(x => x.Dto.ReceiptImage)
                .NotNull().WithMessage("صورة إثبات الدفع مطلوبة.")
                .Must(f => FileUploadValidationRules.IsAllowedImage(f))
                    .WithMessage("نوع الصورة غير مسموح. الأنواع المقبولة: jpg, jpeg, png")
                .Must(f => FileUploadValidationRules.IsSizeWithinLimit(f))
                    .WithMessage("حجم الصورة يتجاوز 5 ميجابايت.")
                .Must(f => FileUploadValidationRules.IsSafeFileName(f))
                    .WithMessage("اسم الملف غير آمن.")
                .When(x => x.Dto.PaymentMethod is PaymentMethod.VodafoneCash
                                               or PaymentMethod.InstaPay);
        }
    }
}