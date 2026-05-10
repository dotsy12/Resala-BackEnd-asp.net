using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.PaymentNumber;
using MediatR;

namespace BackEnd.Application.Features.Admin.PaymentNumbers.Commands.UpsertPaymentNumber
{
    public record UpsertPaymentNumberCommand(UpsertPaymentNumberDto Dto) : IRequest<Result<PaymentNumberDto>>;
}
