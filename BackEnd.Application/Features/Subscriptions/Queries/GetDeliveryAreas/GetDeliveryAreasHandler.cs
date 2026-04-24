// GetDeliveryAreasQuery.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetDeliveryAreas
{
    public class GetDeliveryAreasHandler
        : IRequestHandler<GetDeliveryAreasQuery, Result<IReadOnlyList<DeliveryAreaDto>>>
    {
        private readonly IDeliveryAreaRepository _repo;
        public GetDeliveryAreasHandler(IDeliveryAreaRepository repo) => _repo = repo;

        public async Task<Result<IReadOnlyList<DeliveryAreaDto>>> Handle(
            GetDeliveryAreasQuery request, CancellationToken ct)
        {
            var areas = await _repo.GetAllActiveAsync(ct);
            var result = areas
                .Select(a => new DeliveryAreaDto(a.Id, a.Name))
                .ToList().AsReadOnly();

            return Result<IReadOnlyList<DeliveryAreaDto>>.Success(result);
        }
    }
}