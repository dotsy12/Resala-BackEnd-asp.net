using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetSponsorshipStats
{
    public class GetSponsorshipStatsQueryHandler : IRequestHandler<GetSponsorshipStatsQuery, Result<SponsorshipStatsDto>>
    {
        private readonly IDashboardRepository _repository;

        public GetSponsorshipStatsQueryHandler(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<SponsorshipStatsDto>> Handle(GetSponsorshipStatsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetSponsorshipStatsAsync(cancellationToken);
            return Result<SponsorshipStatsDto>.Success(data);
        }
    }
}
