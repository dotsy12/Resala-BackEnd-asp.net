using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Domain.Enums;
using MediatR;
using System.Collections.Generic;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetDonationTypeStats
{
    public record GetDonationTypeStatsQuery(DashboardPeriod Period) : IRequest<Result<List<DonationTypeStatsDto>>>;
}
