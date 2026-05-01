using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Users.Queries.GetEmergencyContributions
{
    public record GetDonorEmergencyContributionsQuery(int DonorId) : IRequest<Result<IReadOnlyList<EmergencyContributionDto>>>;
}