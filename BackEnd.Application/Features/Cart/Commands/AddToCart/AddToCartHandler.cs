using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Cart;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities;
using BackEnd.Domain.ValueObjects;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Cart.Commands.AddToCart
{
    public class AddToCartHandler : IRequestHandler<AddToCartCommand, Result<CartItemDto>>
    {
        private readonly ICartRepository _cartRepo;
        private readonly IDonorRepository _donorRepo;
        private readonly ISponsorshipRepository _sponsorshipRepo;
        private readonly IEmergencyCaseRepository _emergencyCaseRepo;

        public AddToCartHandler(
            ICartRepository cartRepo,
            IDonorRepository donorRepo,
            ISponsorshipRepository sponsorshipRepo,
            IEmergencyCaseRepository emergencyCaseRepo)
        {
            _cartRepo = cartRepo;
            _donorRepo = donorRepo;
            _sponsorshipRepo = sponsorshipRepo;
            _emergencyCaseRepo = emergencyCaseRepo;
        }

        public async Task<Result<CartItemDto>> Handle(AddToCartCommand request, CancellationToken ct)
        {
            if (request.SponsorshipId == null && request.EmergencyCaseId == null)
                return Result<CartItemDto>.Failure("يجب تحديد الكفالة أو حالة الطوارئ.", ErrorType.BadRequest);

            if (request.SponsorshipId != null && request.EmergencyCaseId != null)
                return Result<CartItemDto>.Failure("لا يمكن تحديد الكفالة وحالة الطوارئ معاً.", ErrorType.BadRequest);

            var donor = await _donorRepo.GetByIdAsync(request.DonorId, ct);
            if (donor == null)
                return Result<CartItemDto>.Failure("المتبرع غير موجود.", ErrorType.NotFound);

            string title = "";
            string? imagePath = null;

            if (request.SponsorshipId.HasValue)
            {
                var sponsorship = await _sponsorshipRepo.GetByIdAsync(request.SponsorshipId.Value, ct);
                if (sponsorship == null)
                    return Result<CartItemDto>.Failure("الكفالة غير موجودة.", ErrorType.NotFound);

                if (!sponsorship.IsActive)
                    return Result<CartItemDto>.Failure("هذه الكفالة غير نشطة حالياً.", ErrorType.BadRequest);

                title = sponsorship.Name;
                imagePath = sponsorship.ImagePath;
            }
            else if (request.EmergencyCaseId.HasValue)
            {
                var emergencyCase = await _emergencyCaseRepo.GetByIdAsync(request.EmergencyCaseId.Value, ct);
                if (emergencyCase == null)
                    return Result<CartItemDto>.Failure("حالة الطوارئ غير موجودة.", ErrorType.NotFound);

                if (!emergencyCase.IsActive)
                    return Result<CartItemDto>.Failure("هذه الحالة غير نشطة حالياً.", ErrorType.BadRequest);

                title = emergencyCase.Title;
                imagePath = emergencyCase.ImagePath;
            }

            var cartItem = await _cartRepo.GetByDonorAndTargetAsync(request.DonorId, request.SponsorshipId, request.EmergencyCaseId, ct);

            if (cartItem == null)
            {
                cartItem = CartItem.Create(request.DonorId, request.SponsorshipId, request.EmergencyCaseId, Money.Zero());
                await _cartRepo.AddAsync(cartItem, ct);
                await _cartRepo.SaveChangesAsync(ct);
            }

            var dto = new CartItemDto
            {
                Id = cartItem.Id,
                SponsorshipId = cartItem.SponsorshipId,
                EmergencyCaseId = cartItem.EmergencyCaseId,
                Title = title,
                ImagePath = imagePath
            };

            return Result<CartItemDto>.Success(dto, "تم إضافة العنصر إلى المفضلة بنجاح.");
        }
    }
}
