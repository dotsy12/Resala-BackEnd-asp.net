using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.FinancialAnalysis;
using MediatR;

namespace BackEnd.Application.Features.FinancialAnalysis.Queries.GetUserFinancialAnalysisById
{
    public class GetUserFinancialAnalysisByIdQuery : IRequest<Result<UserFinancialAnalysisDto>>
    {
        public int DonorId { get; set; }

        public GetUserFinancialAnalysisByIdQuery(int donorId)
        {
            DonorId = donorId;
        }
    }
}
