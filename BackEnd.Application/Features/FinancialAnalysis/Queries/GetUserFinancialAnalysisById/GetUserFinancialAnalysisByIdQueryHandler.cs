using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.FinancialAnalysis;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.FinancialAnalysis.Queries.GetUserFinancialAnalysisById
{
    public class GetUserFinancialAnalysisByIdQueryHandler : IRequestHandler<GetUserFinancialAnalysisByIdQuery, Result<UserFinancialAnalysisDto>>
    {
        private readonly IFinancialAnalysisRepository _repository;

        public GetUserFinancialAnalysisByIdQueryHandler(IFinancialAnalysisRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<UserFinancialAnalysisDto>> Handle(GetUserFinancialAnalysisByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetUserFinancialAnalysisByIdAsync(request.DonorId, cancellationToken);
            if (data == null)
            {
                return Result<UserFinancialAnalysisDto>.Failure("User not found.", ErrorType.NotFound);
            }
            return Result<UserFinancialAnalysisDto>.Success(data);
        }
    }
}
