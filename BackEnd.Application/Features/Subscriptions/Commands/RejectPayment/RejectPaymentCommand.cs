// RejectPaymentCommand.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Commands.RejectPayment
{
    public record RejectPaymentCommand(int PaymentId, int StaffId, string Reason)
        : IRequest<Result<string>>;
}