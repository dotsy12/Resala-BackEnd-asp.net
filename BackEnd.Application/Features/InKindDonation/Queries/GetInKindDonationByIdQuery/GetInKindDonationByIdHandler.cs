using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.InKindDonation.Queries.GetInKindDonationByIdQuery
{
    public class GetInKindDonationByIdHandler
     : IRequestHandler<GetInKindDonationByIdQuery, Result<InKindDonationDto>>
    {
        private readonly IInKindDonationRepository _repo;

        public GetInKindDonationByIdHandler(IInKindDonationRepository repo)
            => _repo = repo;

        public async Task<Result<InKindDonationDto>> Handle(
            GetInKindDonationByIdQuery request, CancellationToken ct)
        {
            var d = await _repo.GetByIdAsync(request.Id, ct);
            if (d is null)
                return Result<InKindDonationDto>.Failure(
                    "التبرع العيني غير موجود.", ErrorType.NotFound);

            return Result<InKindDonationDto>.Success(new InKindDonationDto(
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
            ));
        }
    }
}
