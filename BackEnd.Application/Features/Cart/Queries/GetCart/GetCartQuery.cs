using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Cart;
using MediatR;
using System.Collections.Generic;

namespace BackEnd.Application.Features.Cart.Queries.GetCart
{
    public record GetCartQuery(
        int DonorId
    ) : IRequest<Result<IReadOnlyList<CartItemDto>>>;
}
