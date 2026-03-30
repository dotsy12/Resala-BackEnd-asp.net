using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.ViewModles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Queries.GetEmergencyCaseById
{
    public record GetEmergencyCaseByIdQuery(int Id)
     : IRequest<Result<EmergencyCaseViewModel>>;
}
