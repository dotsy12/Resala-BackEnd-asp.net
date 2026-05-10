using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Basket.Commands.DeleteBasket
{
    public class DeleteBasketCommandHandler : IRequestHandler<DeleteBasketCommand, Result<bool>>
    {
        private readonly IBasketRepository _basketRepository;

        public DeleteBasketCommandHandler(IBasketRepository basketRepository)
            => _basketRepository = basketRepository;

        public async Task<Result<bool>> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DonorId))
                return Result<bool>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized);

            await _basketRepository.DeleteBasketAsync(request.DonorId);

            return Result<bool>.Success(true, "تم حذف السلة بنجاح.");
        }
    }
}
