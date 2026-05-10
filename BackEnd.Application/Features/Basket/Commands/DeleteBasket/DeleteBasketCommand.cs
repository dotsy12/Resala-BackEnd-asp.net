using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Basket.Commands.DeleteBasket
{
    public record DeleteBasketCommand(string DonorId) : IRequest<Result<bool>>;
}
