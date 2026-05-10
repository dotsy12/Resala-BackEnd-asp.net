using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.PaymentNumber;
using MediatR;
using System.Collections.Generic;

namespace BackEnd.Application.Features.Admin.PaymentNumbers.Queries.GetAllPaymentNumbers
{
    public record GetAllPaymentNumbersAdminQuery() : IRequest<Result<IReadOnlyList<PaymentNumberDto>>>;
}
