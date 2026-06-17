using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetUserStats
{
    public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, Result<UserStatsDto>>
    {
        private readonly IDashboardRepository _repository;

        public GetUserStatsQueryHandler(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<UserStatsDto>> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetUserStatsAsync(request.Period, cancellationToken);
            return Result<UserStatsDto>.Success(data);
        }
    }
}
