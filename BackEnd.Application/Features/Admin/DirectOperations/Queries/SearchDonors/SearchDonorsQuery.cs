using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Admin.DirectOperations.Queries.SearchDonors
{
    public record SearchDonorsQuery(string SearchQuery, int PageNumber = 1, int PageSize = 10) 
        : IRequest<Result<PagedResult<DonorSearchResultDto>>>;

    public record DonorSearchResultDto(
        int Id,
        string FullName,
        string Email,
        string PhoneNumber,
        string? Job
    );
}