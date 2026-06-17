using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Admin.DirectOperations.Commands.DirectSubscribe
{
    public record DirectSubscribeCommand(
        int DonorId,
        int SponsorshipId,
        decimal Amount,
        PaymentCycle Cycle,
        int StaffId
    ) : IRequest<Result<int>>;
}