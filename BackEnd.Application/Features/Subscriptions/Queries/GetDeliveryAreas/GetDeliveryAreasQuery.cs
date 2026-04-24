// GetDeliveryAreasQuery.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetDeliveryAreas
{
    public record GetDeliveryAreasQuery : IRequest<Result<IReadOnlyList<DeliveryAreaDto>>>;
}