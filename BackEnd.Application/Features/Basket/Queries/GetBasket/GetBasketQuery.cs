using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Entities.Basket;
using MediatR;

namespace BackEnd.Application.Features.Basket.Queries.GetBasket
{
    public record GetBasketQuery(string DonorId) : IRequest<Result<DonorBasket>>;
}
