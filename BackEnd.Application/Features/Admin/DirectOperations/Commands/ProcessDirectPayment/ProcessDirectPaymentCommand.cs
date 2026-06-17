using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Admin.DirectOperations.Commands.ProcessDirectPayment
{
    public record ProcessDirectPaymentCommand(
        int DonorId,
        int StaffId,
        decimal Amount,
        int? SubscriptionId = null,
        int? EmergencyCaseId = null,
        int? GeneralDonationId = null
    ) : IRequest<Result<int>>;
}