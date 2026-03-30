using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Sponsorship;
using BackEnd.Application.ViewModles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Sponsorship.Commands.UpdateSponsorship
{
    public record UpdateSponsorshipCommand(int Id, UpdateSponsorshipDto Dto)
     : IRequest<Result<SponsorshipViewModel>>;

}
