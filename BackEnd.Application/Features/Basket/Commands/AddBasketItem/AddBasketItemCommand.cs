using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Entities.Basket;
using MediatR;

namespace BackEnd.Application.Features.Basket.Commands.AddBasketItem
{
    public record AddBasketItemCommand(
        string DonorId,
        int TargetId,
        string ItemType,
        decimal Amount,
        string Title,
        string? ImageUrl) : IRequest<Result<DonorBasket>>;
}
