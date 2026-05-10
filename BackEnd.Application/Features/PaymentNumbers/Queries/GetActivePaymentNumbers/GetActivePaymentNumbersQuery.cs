using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.PaymentNumber;
using MediatR;
using System.Collections.Generic;

namespace BackEnd.Application.Features.PaymentNumbers.Queries.GetActivePaymentNumbers
{
    public record GetActivePaymentNumbersQuery() : IRequest<Result<IReadOnlyList<PaymentNumberDto>>>;
}
