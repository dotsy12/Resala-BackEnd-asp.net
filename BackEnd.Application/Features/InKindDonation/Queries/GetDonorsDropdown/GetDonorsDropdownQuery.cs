using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.InKindDonation.Queries.GetDonorsDropdown
{
    public record GetDonorsDropdownQuery(string? Search) : IRequest<Result<IReadOnlyList<DonorDropdownDto>>>;

    public class GetDonorsDropdownHandler 
        : IRequestHandler<GetDonorsDropdownQuery, Result<IReadOnlyList<DonorDropdownDto>>>
    {
        private readonly IDonorRepository _donorRepo;

        public GetDonorsDropdownHandler(IDonorRepository donorRepo)
        {
            _donorRepo = donorRepo;
        }

        public async Task<Result<IReadOnlyList<DonorDropdownDto>>> Handle(
            GetDonorsDropdownQuery request, CancellationToken ct)
        {
            var donors = await _donorRepo.GetDropdownAsync(request.Search, 20, ct);

            var result = donors.Select(d => new DonorDropdownDto(
                Value: d.Id,
                Label: $"{d.FullName.FirstName} {d.FullName.LastName} - {d.PhoneNumber.Value}".Trim()
            )).ToList().AsReadOnly();

            return Result<IReadOnlyList<DonorDropdownDto>>.Success(result);
        }
    }
}
