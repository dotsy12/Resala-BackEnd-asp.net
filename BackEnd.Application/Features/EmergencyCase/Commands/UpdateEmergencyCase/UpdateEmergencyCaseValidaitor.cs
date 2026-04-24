using FluentValidation;
using BackEnd.Application.Common.Validation;

namespace BackEnd.Application.Features.EmergencyCase.Commands.UpdateEmergencyCase
{
    public class UpdateEmergencyCaseValidator : AbstractValidator<UpdateEmergencyCaseCommand>
    {
        public UpdateEmergencyCaseValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("معرف الحالة غير صحيح.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("العنوان مطلوب.")
                .MaximumLength(200)
                .WithMessage("العنوان لا يتجاوز 200 حرف.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("الوصف مطلوب.");

            RuleFor(x => x.UrgencyLevel)
                .IsInEnum()
                .WithMessage("مستوى الحالة غير صحيح.");

            RuleFor(x => x.RequiredAmount)
                .GreaterThan(0)
                .When(x => x.RequiredAmount.HasValue)
                .WithMessage("المبلغ المطلوب يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.Attachment)
                .Must(file => file is null || FileUploadValidationRules.IsAllowedImageOrPdf(file))
                .WithMessage("صيغة الملف غير مدعومة. المسموح: jpg, jpeg, png, pdf.");

            RuleFor(x => x.Attachment)
                .Must(file => file is null || FileUploadValidationRules.IsSizeWithinLimit(file))
                .WithMessage("حجم الملف يجب ألا يتجاوز 5MB.");

            RuleFor(x => x.Attachment)
                .Must(file => file is null || FileUploadValidationRules.IsSafeFileName(file))
                .WithMessage("اسم الملف غير آمن.");
        }
    }
}