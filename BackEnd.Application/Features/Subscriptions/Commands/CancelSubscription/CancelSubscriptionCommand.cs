// CancelSubscriptionCommand.cs + Handler + Validator
using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Commands.CancelSubscription
{
    public record CancelSubscriptionCommand(int SubscriptionId, int DonorId, string? Reason)
        : IRequest<Result<string>>;
}