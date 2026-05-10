using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using MediatR;
using System.Collections.Generic;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetMonthlyDonationTrend
{
    public record GetMonthlyDonationTrendQuery : IRequest<Result<List<MonthlyDonationTrendDto>>>;
}
