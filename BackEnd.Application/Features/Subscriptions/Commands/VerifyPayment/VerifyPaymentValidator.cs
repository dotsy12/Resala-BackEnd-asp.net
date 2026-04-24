// VerifyPaymentCommand.cs + Handler
using FluentValidation;

namespace BackEnd.Application.Features.Subscriptions.Commands.VerifyPayment
{
    public class VerifyPaymentValidator : AbstractValidator<VerifyPaymentCommand>
    {
        public VerifyPaymentValidator()
        {
            RuleFor(x => x.PaymentId).GreaterThan(0);
            RuleFor(x => x.StaffId).GreaterThan(0);
        }
    }
}