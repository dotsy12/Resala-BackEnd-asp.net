// CancelSubscriptionCommand.cs + Handler + Validator
using FluentValidation;

namespace BackEnd.Application.Features.Subscriptions.Commands.CancelSubscription
{
    public class CancelSubscriptionValidator
        : AbstractValidator<CancelSubscriptionCommand>
    {
        public CancelSubscriptionValidator()
        {
            RuleFor(x => x.SubscriptionId).GreaterThan(0);
            RuleFor(x => x.DonorId).GreaterThan(0);
        }
    }
}