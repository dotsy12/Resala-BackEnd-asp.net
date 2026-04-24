// GetMySubscriptionsQuery.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetMySubscriptions
{
    public record GetMySubscriptionsQuery(int DonorId)
        : IRequest<Result<IReadOnlyList<SubscriptionDto>>>;
}