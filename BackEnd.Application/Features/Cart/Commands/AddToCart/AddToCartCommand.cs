using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Cart;
using MediatR;

namespace BackEnd.Application.Features.Cart.Commands.AddToCart
{
    public record AddToCartCommand(
        int DonorId,
        int? SponsorshipId,
        int? EmergencyCaseId,
        decimal Amount
    ) : IRequest<Result<CartItemDto>>;
}
