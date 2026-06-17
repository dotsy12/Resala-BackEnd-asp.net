using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetMonthlyDonationTrend
{
    public class GetMonthlyDonationTrendQueryHandler : IRequestHandler<GetMonthlyDonationTrendQuery, Result<List<MonthlyDonationTrendDto>>>
    {
        private readonly IDashboardRepository _repository;

        public GetMonthlyDonationTrendQueryHandler(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<MonthlyDonationTrendDto>>> Handle(GetMonthlyDonationTrendQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetMonthlyDonationTrendAsync(request.Period, cancellationToken);
            return Result<List<MonthlyDonationTrendDto>>.Success(data);
        }
    }
}
