using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using MediatR;

public record GetSubscriptionByIdQuery(int SubscriptionId, int RequesterId, string RequesterRole)
    : IRequest<Result<SubscriptionDto>>;
