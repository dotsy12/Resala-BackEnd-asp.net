using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase
{
    public record CreateEmergencyCaseCommand(string? Title,
        string? Description,
        UrgencyLevel UrgencyLevel,
        decimal RequiredAmount,
        string? ImageUrl)
     : IRequest<Result<EmergencyCaseViewModel>>;
}
