using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.EmergencyCase;
using MediatR;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public record DonateElectronicToEmergencyCaseCommand(
        int CaseId,
        int DonorId,
        ElectronicEmergencyDonationDto Dto
    ) : IRequest<Result<EmergencyDonationResponse>>;
}
