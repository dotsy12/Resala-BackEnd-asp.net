// GetPendingPaymentsQuery.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetPendingPayments
{
    public record GetPendingPaymentsQuery : IRequest<Result<IReadOnlyList<PaymentRequestSummaryDto>>>;
    public record GetPendingPaymentsByMethodQuery(PaymentMethod Method)
        : IRequest<Result<IReadOnlyList<PaymentRequestSummaryDto>>>;

   
}