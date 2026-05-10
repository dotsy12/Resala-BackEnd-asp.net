using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetOverview
{
    public class GetDashboardOverviewQueryHandler : IRequestHandler<GetDashboardOverviewQuery, Result<DashboardOverviewDto>>
    {
        private readonly IDashboardRepository _repository;

        public GetDashboardOverviewQueryHandler(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<DashboardOverviewDto>> Handle(GetDashboardOverviewQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetOverviewAsync(cancellationToken);
            return Result<DashboardOverviewDto>.Success(data);
        }
    }
}
