using BackEnd.Application.Features.Sponsorship.Commands.Create;
using BackEnd.Application.Common.Validation;
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

            RuleFor(x => x.Dto.ImageFile)
                .Must(file => file is null || FileUploadValidationRules.IsAllowedImage(file))
                .WithMessage("صيغة الملف غير مدعومة. المسموح: jpg, jpeg, png.");

            RuleFor(x => x.Dto.ImageFile)
                .Must(file => file is null || FileUploadValidationRules.IsSizeWithinLimit(file))
                .WithMessage("حجم الملف يجب ألا يتجاوز 5MB.");

            RuleFor(x => x.Dto.ImageFile)
                .Must(file => file is null || FileUploadValidationRules.IsSafeFileName(file))
                .WithMessage("اسم الملف غير آمن.");
        }
    }
}