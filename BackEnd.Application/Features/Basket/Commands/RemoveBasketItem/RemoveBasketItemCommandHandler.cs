using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Basket;
using MediatR;

namespace BackEnd.Application.Features.Basket.Commands.RemoveBasketItem
{
    public class RemoveBasketItemCommandHandler : IRequestHandler<RemoveBasketItemCommand, Result<DonorBasket>>
    {
        private readonly IBasketRepository _basketRepository;

        public RemoveBasketItemCommandHandler(IBasketRepository basketRepository)
            => _basketRepository = basketRepository;

        public async Task<Result<DonorBasket>> Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DonorId))
                return Result<DonorBasket>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized);

            if (request.ItemId == Guid.Empty)
                return Result<DonorBasket>.Failure("معرف عنصر السلة مطلوب.", ErrorType.BadRequest);

            var basket = await _basketRepository.GetBasketAsync(request.DonorId);

            if (basket is null)
                return Result<DonorBasket>.Failure("السلة غير موجودة.", ErrorType.NotFound);

            var item = basket.Items.FirstOrDefault(i => i.Id == request.ItemId);

            if (item is null)
                return Result<DonorBasket>.Failure("عنصر السلة غير موجود.", ErrorType.NotFound);

            basket.Items.Remove(item);
            await _basketRepository.UpdateBasketAsync(basket);

            return Result<DonorBasket>.Success(basket, "تم حذف العنصر من السلة بنجاح.");
        }
    }
}
