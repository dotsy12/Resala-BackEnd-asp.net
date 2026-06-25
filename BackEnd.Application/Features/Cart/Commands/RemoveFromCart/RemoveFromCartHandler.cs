using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Cart.Commands.RemoveFromCart
{
    public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand, Result<bool>>
    {
        private readonly ICartRepository _cartRepo;

        public RemoveFromCartHandler(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<Result<bool>> Handle(RemoveFromCartCommand request, CancellationToken ct)
        {
            var cartItem = await _cartRepo.GetByIdAsync(request.CartItemId, ct);
            if (cartItem == null)
                return Result<bool>.Failure("عنصر السلة غير موجود.", ErrorType.NotFound);

            if (cartItem.DonorId != request.DonorId)
                return Result<bool>.Failure("غير مصرح لك بحذف هذا العنصر.", ErrorType.Forbidden);

            _cartRepo.Remove(cartItem);
            await _cartRepo.SaveChangesAsync(ct);

            return Result<bool>.Success(true, "تم حذف العنصر من السلة بنجاح.");
        }
    }
}
