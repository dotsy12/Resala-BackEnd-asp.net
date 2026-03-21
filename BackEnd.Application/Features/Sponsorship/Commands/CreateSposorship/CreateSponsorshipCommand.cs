using BackEnd.Application.Dtos.Sponsorship;
using BackEnd.Application.ViewModles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Sponsorship.Commands.Create
{
    public record CreateSponsorshipCommand(CreateSponsorshipDto Dto) : IRequest<SponsorshipViewModel>;

}
