using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.InKindDonation.Queries.GetDonorInKindDonationsQuery
{
    public record GetDonorInKindDonationsQuery(int DonorId)
        : IRequest<Result<IReadOnlyList<InKindDonationDto>>>;
}
