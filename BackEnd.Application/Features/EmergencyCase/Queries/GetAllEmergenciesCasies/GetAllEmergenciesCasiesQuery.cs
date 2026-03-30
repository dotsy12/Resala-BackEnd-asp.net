using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.ViewModles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Queries.GetAllEmergenciesCasies
{
    public record GetAllEmergencyCasesQuery
      : IRequest<Result<IEnumerable<EmergencyCaseViewModel>>>;
}
