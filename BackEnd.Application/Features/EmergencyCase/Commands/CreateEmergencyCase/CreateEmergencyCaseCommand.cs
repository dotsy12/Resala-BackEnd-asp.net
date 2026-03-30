using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.EmergencyCase;
using BackEnd.Application.ViewModles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase
{
    public record CreateEmergencyCaseCommand(CreateEmergencyCaseDto Dto)
     : IRequest<Result<EmergencyCaseViewModel>>;
}
