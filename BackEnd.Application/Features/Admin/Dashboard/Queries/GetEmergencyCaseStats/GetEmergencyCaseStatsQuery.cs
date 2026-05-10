using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using MediatR;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetEmergencyCaseStats
{
    public record GetEmergencyCaseStatsQuery : IRequest<Result<EmergencyCaseStatsDto>>;
}
