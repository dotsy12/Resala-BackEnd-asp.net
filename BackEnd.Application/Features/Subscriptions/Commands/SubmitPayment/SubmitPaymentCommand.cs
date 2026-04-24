// SubmitPaymentCommand.cs
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Commands.SubmitPayment
{
    public record SubmitPaymentCommand(int SubscriptionId, int DonorId, SubmitPaymentDto Dto)
        : IRequest<Result<PaymentRequestSummaryDto>>;
}