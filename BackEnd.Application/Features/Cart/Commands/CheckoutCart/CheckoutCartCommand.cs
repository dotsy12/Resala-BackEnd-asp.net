using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Cart;
using MediatR;

namespace BackEnd.Application.Features.Cart.Commands.CheckoutCart
{
    public record CheckoutCartCommand(
        int DonorId,
        CheckoutCartDto Dto
    ) : IRequest<Result<bool>>;
}
