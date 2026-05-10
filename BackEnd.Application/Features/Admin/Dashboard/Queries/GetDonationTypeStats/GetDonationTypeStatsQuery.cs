using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using MediatR;
using System.Collections.Generic;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetDonationTypeStats
{
    public record GetDonationTypeStatsQuery : IRequest<Result<List<DonationTypeStatsDto>>>;
}
