using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.InKindDonation.Commands.UpdateInKindDonation
{
    public record UpdateInKindDonationCommand(
          int Id,
          string DonationTypeName,
          int Quantity,
          string? Description
      ) : IRequest<Result<InKindDonationDto>>;
}
