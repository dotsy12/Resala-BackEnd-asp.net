using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.FinancialAnalysis;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.FinancialAnalysis.Queries.GetGlobalDashboardAnalytics
{
    public class GetGlobalDashboardAnalyticsQueryHandler : IRequestHandler<GetGlobalDashboardAnalyticsQuery, Result<GlobalDashboardAnalyticsDto>>
    {
        private readonly IFinancialAnalysisRepository _repository;

        public GetGlobalDashboardAnalyticsQueryHandler(IFinancialAnalysisRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<GlobalDashboardAnalyticsDto>> Handle(GetGlobalDashboardAnalyticsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetGlobalDashboardAnalyticsAsync(cancellationToken);
            return Result<GlobalDashboardAnalyticsDto>.Success(data);
        }
    }
}
