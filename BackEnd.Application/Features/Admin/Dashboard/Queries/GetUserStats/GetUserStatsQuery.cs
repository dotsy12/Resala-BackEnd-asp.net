using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetUserStats
{
    public record GetUserStatsQuery(DashboardPeriod Period) : IRequest<Result<UserStatsDto>>;
}
