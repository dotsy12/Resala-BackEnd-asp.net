using BackEnd.Application.Common.ResponseFormat;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DeleteEmergencyCase
{
    public record DeleteEmergencyCaseCommand(int id) : IRequest<Result<bool>>;
}
  