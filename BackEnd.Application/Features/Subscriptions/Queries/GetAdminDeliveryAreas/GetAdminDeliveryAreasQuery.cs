using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetAdminDeliveryAreas
{
    public record GetAdminDeliveryAreasQuery : IRequest<Result<IReadOnlyList<DeliveryAreaDto>>>;

    public class GetAdminDeliveryAreasHandler 
        : IRequestHandler<GetAdminDeliveryAreasQuery, Result<IReadOnlyList<DeliveryAreaDto>>>
    {
        private readonly IDeliveryAreaRepository _repo;

        public GetAdminDeliveryAreasHandler(IDeliveryAreaRepository repo) => _repo = repo;

        public async Task<Result<IReadOnlyList<DeliveryAreaDto>>> Handle(
            GetAdminDeliveryAreasQuery request, CancellationToken ct)
        {
            var areas = await _repo.GetAllAsync(ct);
            var result = areas.Select(a => new DeliveryAreaDto(
                a.Id,
                a.Name,
                a.Governorate,
                a.City,
                a.IsActive
            )).ToList().AsReadOnly();

            return Result<IReadOnlyList<DeliveryAreaDto>>.Success(result);
        }
    }
}
