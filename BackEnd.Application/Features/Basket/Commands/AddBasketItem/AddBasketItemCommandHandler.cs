using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Basket;
using MediatR;

namespace BackEnd.Application.Features.Basket.Commands.AddBasketItem
{
    public class AddBasketItemCommandHandler : IRequestHandler<AddBasketItemCommand, Result<DonorBasket>>
    {
        private readonly IBasketRepository _basketRepository;

        public AddBasketItemCommandHandler(IBasketRepository basketRepository)
            => _basketRepository = basketRepository;

        public async Task<Result<DonorBasket>> Handle(AddBasketItemCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DonorId))
                return Result<DonorBasket>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized);

            if (request.TargetId <= 0)
                return Result<DonorBasket>.Failure("معرف الهدف مطلوب.", ErrorType.BadRequest);

            if (!IsSupportedItemType(request.ItemType))
                return Result<DonorBasket>.Failure("نوع عنصر السلة غير مدعوم.", ErrorType.BadRequest);

            if (request.Amount <= 0)
                return Result<DonorBasket>.Failure("قيمة التبرع يجب أن تكون أكبر من صفر.", ErrorType.BadRequest);

            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<DonorBasket>.Failure("عنوان عنصر السلة مطلوب.", ErrorType.BadRequest);

            var basket = await _basketRepository.GetBasketAsync(request.DonorId)
                         ?? new DonorBasket { Id = request.DonorId };

            basket.Items.Add(new BasketItem
            {
                Id = Guid.NewGuid(),
                TargetId = request.TargetId,
                ItemType = request.ItemType.Trim(),
                Amount = request.Amount,
                Title = request.Title.Trim(),
                ImageUrl = request.ImageUrl?.Trim()
            });

            await _basketRepository.UpdateBasketAsync(basket);

            return Result<DonorBasket>.Success(basket, "تمت إضافة العنصر إلى السلة بنجاح.");
        }

        private static bool IsSupportedItemType(string itemType)
            => itemType.Equals("Sponsorship", StringComparison.OrdinalIgnoreCase)
               || itemType.Equals("EmergencyCase", StringComparison.OrdinalIgnoreCase);
    }
}
