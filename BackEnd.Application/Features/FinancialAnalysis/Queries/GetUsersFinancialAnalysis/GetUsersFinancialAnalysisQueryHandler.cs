using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.FinancialAnalysis;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.FinancialAnalysis.Queries.GetUsersFinancialAnalysis
{
    public class GetUsersFinancialAnalysisQueryHandler : IRequestHandler<GetUsersFinancialAnalysisQuery, Result<PagedResult<UserFinancialAnalysisDto>>>
    {
        private readonly IFinancialAnalysisRepository _repository;

        public GetUsersFinancialAnalysisQueryHandler(IFinancialAnalysisRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<PagedResult<UserFinancialAnalysisDto>>> Handle(GetUsersFinancialAnalysisQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _repository.GetUsersFinancialAnalysisAsync(
                request.Search,
                request.StatusFilter,
                request.ActiveSubscriptions,
                request.DelayedUsers,
                request.EmergencyDonors,
                request.InKindDonors,
                request.SortBy,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var pagedResult = new PagedResult<UserFinancialAnalysisDto>(items, totalCount, request.PageNumber, request.PageSize);
            return Result<PagedResult<UserFinancialAnalysisDto>>.Success(pagedResult);
        }
    }
}
