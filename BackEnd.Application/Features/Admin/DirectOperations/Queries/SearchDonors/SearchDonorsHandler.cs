using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Admin.DirectOperations.Queries.SearchDonors
{
    public class SearchDonorsHandler : IRequestHandler<SearchDonorsQuery, Result<PagedResult<DonorSearchResultDto>>>
    {
        private readonly IDonorRepository _donorRepo;

        public SearchDonorsHandler(IDonorRepository donorRepo)
        {
            _donorRepo = donorRepo;
        }

        public async Task<Result<PagedResult<DonorSearchResultDto>>> Handle(SearchDonorsQuery request, CancellationToken ct)
        {
            var (items, totalCount) = await _donorRepo.GetPagedAsync(request.SearchQuery, request.PageNumber, request.PageSize, ct);

            var dtos = items.Select(d => new DonorSearchResultDto(
                d.Id,
                d.FullName.FullName,
                d.Email.Value,
                d.PhoneNumber.Value,
                d.Job
            )).ToList();

            return Result<PagedResult<DonorSearchResultDto>>.Success(new PagedResult<DonorSearchResultDto>(
                dtos, totalCount, request.PageNumber, request.PageSize));
        }
    }
}