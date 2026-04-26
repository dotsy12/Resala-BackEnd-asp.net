using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.EmergencyCase;
using MediatR;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public record DonateBranchToEmergencyCaseCommand(
        int CaseId,
        int DonorId,
        BranchEmergencyDonationDto Dto
    ) : IRequest<Result<EmergencyDonationResponse>>;
}
