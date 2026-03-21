using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace BackEnd.Application.Features.Sponsorship.Commands.DeleteSponsorship
{
     
    public record DeleteSponsorshipCommand(int Id) : IRequest<Unit>;
    
}
