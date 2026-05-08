using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.FinancialAnalysis;
using MediatR;

namespace BackEnd.Application.Features.FinancialAnalysis.Queries.GetGlobalDashboardAnalytics
{
    public class GetGlobalDashboardAnalyticsQuery : IRequest<Result<GlobalDashboardAnalyticsDto>>
    {
    }
}
