using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetSponsorshipStats
{
    public record GetSponsorshipStatsQuery(DashboardPeriod Period) : IRequest<Result<SponsorshipStatsDto>>;
}
