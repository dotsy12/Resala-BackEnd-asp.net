using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.InKindDonation.Queries.GetAllInKindDonationsQuery
{
    public class GetAllInKindDonationsHandler
     : IRequestHandler<GetAllInKindDonationsQuery, Result<IReadOnlyList<InKindDonationDto>>>
    {
        private readonly IInKindDonationRepository _repo;

        public GetAllInKindDonationsHandler(IInKindDonationRepository repo)
            => _repo = repo;

        public async Task<Result<IReadOnlyList<InKindDonationDto>>> Handle(
            GetAllInKindDonationsQuery request, CancellationToken ct)
        {
            var donations = await _repo.GetAllAsync(ct);

            var result = donations.Select(d => new InKindDonationDto(
                Id: d.Id,
                DonorId: d.DonorId,
                DonorName: d.Donor is not null
                    ? $"{d.Donor.FullName.FirstName} {d.Donor.FullName.LastName}".Trim() : "",
                DonationTypeName: d.DonationTypeName,
                Quantity: d.Quantity,
                Description: d.Description,
                RecordedByStaffId: d.RecordedByStaffId,
                RecordedByStaffName: d.RecordedBy is not null
                    ? $"{d.RecordedBy.FullName.FirstName}   {d.RecordedBy.FullName.LastName}".Trim() : "",
                RecordedAt: d.RecordedAt,
                CreatedOn: d.CreatedOn
            )).ToList().AsReadOnly();

            return Result<IReadOnlyList<InKindDonationDto>>.Success(result);
        }
    }
}
