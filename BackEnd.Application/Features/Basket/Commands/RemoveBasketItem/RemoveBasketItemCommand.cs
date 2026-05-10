using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Entities.Basket;
using MediatR;

namespace BackEnd.Application.Features.Basket.Commands.RemoveBasketItem
{
    public record RemoveBasketItemCommand(string DonorId, Guid ItemId) : IRequest<Result<DonorBasket>>;
}
