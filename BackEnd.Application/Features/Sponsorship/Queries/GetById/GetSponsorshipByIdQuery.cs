using BackEnd.Application.ViewModles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Sponsorship.Queries.GetById
{
    public record GetSponsorshipByIdQuery(int id): IRequest<SponsorshipViewModel>;
   
}
