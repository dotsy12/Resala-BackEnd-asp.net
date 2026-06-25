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
            if (request.Amount <= 0)
                return Result<CartItemDto>.Failure("مبلغ التبرع يجب أن يكون أكبر من الصفر.", ErrorType.BadRequest);

            if (request.SponsorshipId == null && request.EmergencyCaseId == null)
                return Result<CartItemDto>.Failure("يجب تحديد الكفالة أو حالة الطوارئ.", ErrorType.BadRequest);

            if (request.SponsorshipId != null && request.EmergencyCaseId != null)
                return Result<CartItemDto>.Failure("لا يمكن تحديد الكفالة وحالة الطوارئ معاً.", ErrorType.BadRequest);

            var donor = await _donorRepo.GetByIdAsync(request.DonorId, ct);
            if (donor == null)
                return Result<CartItemDto>.Failure("المتبرع غير موجود.", ErrorType.NotFound);

            string title = "";
            string? imagePath = null;
            decimal? targetGoal = null;
            decimal? targetCollected = null;
            bool isCompleted = false;
            string targetType = "";

            if (request.SponsorshipId.HasValue)
            {
                var sponsorship = await _sponsorshipRepo.GetByIdAsync(request.SponsorshipId.Value, ct);
                if (sponsorship == null)
                    return Result<CartItemDto>.Failure("الكفالة غير موجودة.", ErrorType.NotFound);

                if (!sponsorship.IsActive)
                    return Result<CartItemDto>.Failure("هذه الكفالة غير نشطة حالياً.", ErrorType.BadRequest);

                title = sponsorship.Name;
                imagePath = sponsorship.ImagePath;
                targetGoal = sponsorship.FinancialGoal?.Amount;
                targetCollected = sponsorship.TotalCollected.Amount;
                isCompleted = sponsorship.FinancialGoal != null && sponsorship.TotalCollected.Amount >= sponsorship.FinancialGoal.Amount;
                targetType = "Sponsorship";
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
                targetGoal = emergencyCase.RequiredAmount.Amount;
                targetCollected = emergencyCase.CollectedAmount.Amount;
                isCompleted = emergencyCase.IsCompleted;
                targetType = "EmergencyCase";
            }

            var amount = new Money(request.Amount);
            var cartItem = await _cartRepo.GetByDonorAndTargetAsync(request.DonorId, request.SponsorshipId, request.EmergencyCaseId, ct);

            if (cartItem != null)
            {
                cartItem.UpdateAmount(amount);
                _cartRepo.Update(cartItem);
            }
            else
            {
                cartItem = CartItem.Create(request.DonorId, request.SponsorshipId, request.EmergencyCaseId, amount);
                await _cartRepo.AddAsync(cartItem, ct);
            }

            await _cartRepo.SaveChangesAsync(ct);

            var dto = new CartItemDto
            {
                Id = cartItem.Id,
                DonorId = cartItem.DonorId,
                SponsorshipId = cartItem.SponsorshipId,
                EmergencyCaseId = cartItem.EmergencyCaseId,
                DonationAmount = cartItem.DonationAmount.Amount,
                Currency = cartItem.DonationAmount.Currency,
                CreatedAt = cartItem.CreatedAt,
                TargetType = targetType,
                Title = title,
                ImagePath = imagePath,
                TargetGoalAmount = targetGoal,
                TargetCollectedAmount = targetCollected,
                IsCompleted = isCompleted
            };

            return Result<CartItemDto>.Success(dto, "تم إضافة العنصر إلى السلة بنجاح.");
        }
    }
}
