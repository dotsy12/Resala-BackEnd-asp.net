using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using BackEnd.Application.ViewModles;

namespace BackEnd.Application.Features.Sponsorship.Queries.GetAll
{


    public record GetAllSponsorshipsQuery : IRequest<IEnumerable<SponsorshipViewModel>>;
    
}
