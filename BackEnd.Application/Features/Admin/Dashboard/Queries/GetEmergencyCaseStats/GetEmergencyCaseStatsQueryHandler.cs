using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetEmergencyCaseStats
{
    public class GetEmergencyCaseStatsQueryHandler : IRequestHandler<GetEmergencyCaseStatsQuery, Result<EmergencyCaseStatsDto>>
    {
        private readonly IDashboardRepository _repository;

        public GetEmergencyCaseStatsQueryHandler(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<EmergencyCaseStatsDto>> Handle(GetEmergencyCaseStatsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetEmergencyCaseStatsAsync(cancellationToken);
            return Result<EmergencyCaseStatsDto>.Success(data);
        }
    }
}
