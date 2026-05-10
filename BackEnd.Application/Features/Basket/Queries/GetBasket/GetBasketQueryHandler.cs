using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Basket;
using MediatR;

namespace BackEnd.Application.Features.Basket.Queries.GetBasket
{
    public class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, Result<DonorBasket>>
    {
        private readonly IBasketRepository _basketRepository;

        public GetBasketQueryHandler(IBasketRepository basketRepository)
            => _basketRepository = basketRepository;

        public async Task<Result<DonorBasket>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DonorId))
                return Result<DonorBasket>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized);

            var basket = await _basketRepository.GetBasketAsync(request.DonorId);
            basket ??= new DonorBasket { Id = request.DonorId };

            return Result<DonorBasket>.Success(basket);
        }
    }
}
