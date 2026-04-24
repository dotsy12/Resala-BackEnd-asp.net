// CreateSubscriptionCommand.cs
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Commands.CreateSubscription
{
    public record CreateSubscriptionCommand(int DonorId, CreateSubscriptionDto Dto)
        : IRequest<Result<SubscriptionDto>>;
}