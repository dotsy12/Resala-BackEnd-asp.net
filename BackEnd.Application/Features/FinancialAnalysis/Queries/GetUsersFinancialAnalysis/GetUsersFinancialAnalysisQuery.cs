using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.FinancialAnalysis;
using MediatR;

namespace BackEnd.Application.Features.FinancialAnalysis.Queries.GetUsersFinancialAnalysis
{
    public class GetUsersFinancialAnalysisQuery : IRequest<Result<PagedResult<UserFinancialAnalysisDto>>>
    {
        public string? Search { get; set; }
        public string? StatusFilter { get; set; }
        public bool? ActiveSubscriptions { get; set; }
        public bool? DelayedUsers { get; set; }
        public bool? EmergencyDonors { get; set; }
        public bool? InKindDonors { get; set; }
        public string? SortBy { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
