using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using MediatR;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase
{
    public record EmergencyDonationResponse(
        int PaymentId,
        int CaseId,
        decimal Amount,
        string Method,
        string Status);

    public record DonateToEmergencyCaseCommand(
        int CaseId,
        int DonorId,
        SubmitPaymentDto Dto
    ) : IRequest<Result<EmergencyDonationResponse>>;
}
