using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using MediatR;

namespace BackEnd.Application.Features.Admin.Dashboard.Queries.GetSponsorshipStats
{
    public record GetSponsorshipStatsQuery : IRequest<Result<SponsorshipStatsDto>>;
}
