using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Cart;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Cart.Queries.GetCart
{
    public class GetCartHandler : IRequestHandler<GetCartQuery, Result<IReadOnlyList<CartItemDto>>>
    {
        private readonly ICartRepository _cartRepo;

        public GetCartHandler(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<Result<IReadOnlyList<CartItemDto>>> Handle(GetCartQuery request, CancellationToken ct)
        {
            var cartItems = await _cartRepo.GetByDonorIdAsync(request.DonorId, ct);

            var dtos = cartItems.Select(c =>
            {
                string title = "";
                string? imagePath = null;

                if (c.SponsorshipId.HasValue && c.Sponsorship != null)
                {
                    title = c.Sponsorship.Name;
                    imagePath = c.Sponsorship.ImagePath;
                }
                else if (c.EmergencyCaseId.HasValue && c.EmergencyCase != null)
                {
                    title = c.EmergencyCase.Title;
                    imagePath = c.EmergencyCase.ImagePath;
                }

                return new CartItemDto
                {
                    Id = c.Id,
                    SponsorshipId = c.SponsorshipId,
                    EmergencyCaseId = c.EmergencyCaseId,
                    Title = title,
                    ImagePath = imagePath
                };
            }).ToList();

            return Result<IReadOnlyList<CartItemDto>>.Success(dtos, "تم استرجاع عناصر السلة بنجاح.");
        }
    }
}
