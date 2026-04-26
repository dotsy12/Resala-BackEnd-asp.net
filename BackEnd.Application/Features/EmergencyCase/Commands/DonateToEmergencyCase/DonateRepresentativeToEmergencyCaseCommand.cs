using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.EmergencyCase;
using MediatR;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public record DonateRepresentativeToEmergencyCaseCommand(
        int CaseId,
        int DonorId,
        RepresentativeEmergencyDonationDto Dto
    ) : IRequest<Result<EmergencyDonationResponse>>;
}
