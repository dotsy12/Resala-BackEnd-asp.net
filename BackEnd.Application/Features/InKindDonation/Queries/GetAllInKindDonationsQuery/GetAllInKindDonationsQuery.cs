using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.InKindDonation.Queries.GetAllInKindDonationsQuery
{
    public record GetAllInKindDonationsQuery : IRequest<Result<IReadOnlyList<InKindDonationDto>>>;

}
