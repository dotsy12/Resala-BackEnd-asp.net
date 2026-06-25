using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Cart.Commands.RemoveFromCart
{
    public record RemoveFromCartCommand(
        int DonorId,
        int CartItemId
    ) : IRequest<Result<bool>>;
}
