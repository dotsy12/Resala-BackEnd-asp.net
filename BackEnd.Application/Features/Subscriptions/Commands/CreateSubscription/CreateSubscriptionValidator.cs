// CreateSubscriptionValidator.cs
using FluentValidation;

namespace BackEnd.Application.Features.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionCommand>
    {
        public CreateSubscriptionValidator()
        {
            RuleFor(x => x.DonorId)
                .GreaterThan(0).WithMessage("يجب تحديد المتبرع.");

            RuleFor(x => x.Dto.SponsorshipId)
                .GreaterThan(0).WithMessage("يجب اختيار برنامج كفالة.");

            RuleFor(x => x.Dto.Amount)
                .GreaterThan(0).WithMessage("المبلغ يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(1_000_000).WithMessage("المبلغ مرتفع جداً.");

            RuleFor(x => x.Dto.PaymentCycle)
                .IsInEnum().WithMessage("دورة الدفع غير صحيحة.");
        }
    }
}