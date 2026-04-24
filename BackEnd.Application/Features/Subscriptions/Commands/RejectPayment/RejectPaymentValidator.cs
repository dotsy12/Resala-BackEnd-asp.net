// RejectPaymentCommand.cs + Handler
using FluentValidation;

namespace BackEnd.Application.Features.Subscriptions.Commands.RejectPayment
{
    public class RejectPaymentValidator : AbstractValidator<RejectPaymentCommand>
    {
        public RejectPaymentValidator()
        {
            RuleFor(x => x.PaymentId).GreaterThan(0);
            RuleFor(x => x.StaffId).GreaterThan(0);
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("سبب الرفض مطلوب.")
                .MaximumLength(500);
        }
    }
}