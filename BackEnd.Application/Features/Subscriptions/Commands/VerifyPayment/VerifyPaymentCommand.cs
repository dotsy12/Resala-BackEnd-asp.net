// VerifyPaymentCommand.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Commands.VerifyPayment
{
    public record VerifyPaymentCommand(int PaymentId, int StaffId)
        : IRequest<Result<string>>;
}