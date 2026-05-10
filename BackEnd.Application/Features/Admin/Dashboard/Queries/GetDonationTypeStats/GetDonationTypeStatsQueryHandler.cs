using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetDonationTypeStats
{
    public class GetDonationTypeStatsQueryHandler : IRequestHandler<GetDonationTypeStatsQuery, Result<List<DonationTypeStatsDto>>>
    {
        private readonly IDashboardRepository _repository;

        public GetDonationTypeStatsQueryHandler(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<DonationTypeStatsDto>>> Handle(GetDonationTypeStatsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetDonationTypeStatsAsync(cancellationToken);
            return Result<List<DonationTypeStatsDto>>.Success(data);
        }
    }
}
