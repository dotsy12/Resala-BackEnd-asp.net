using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.InKindDonation.Queries.GetDonorsLookup
{
    public record GetDonorsLookupQuery(
        string? Search,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<Result<PagedResult<DonorLookupDto>>>;

    public class GetDonorsLookupHandler 
        : IRequestHandler<GetDonorsLookupQuery, Result<PagedResult<DonorLookupDto>>>
    {
        private readonly IDonorRepository _donorRepo;

        public GetDonorsLookupHandler(IDonorRepository donorRepo)
        {
            _donorRepo = donorRepo;
        }

        public async Task<Result<PagedResult<DonorLookupDto>>> Handle(
            GetDonorsLookupQuery request, CancellationToken ct)
        {
            var (items, totalCount) = await _donorRepo.GetPagedAsync(
                request.Search, request.PageNumber, request.PageSize, ct);

            var dtoList = items.Select(d => new DonorLookupDto(
                Id: d.Id,
                FullName: $"{d.FullName.FirstName} {d.FullName.LastName}".Trim(),
                PhoneNumber: d.PhoneNumber.Value,
                Email: d.Email.Value
            )).ToList();

            var pagedResult = new PagedResult<DonorLookupDto>(
                dtoList, totalCount, request.PageNumber, request.PageSize);

            return Result<PagedResult<DonorLookupDto>>.Success(pagedResult);
        }
    }
}
