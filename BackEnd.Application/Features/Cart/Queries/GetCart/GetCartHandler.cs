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
                decimal? targetGoal = null;
                decimal? targetCollected = null;
                bool isCompleted = false;
                string targetType = "";

                if (c.SponsorshipId.HasValue && c.Sponsorship != null)
                {
                    title = c.Sponsorship.Name;
                    imagePath = c.Sponsorship.ImagePath;
                    targetGoal = c.Sponsorship.FinancialGoal?.Amount;
                    targetCollected = c.Sponsorship.TotalCollected.Amount;
                    isCompleted = c.Sponsorship.FinancialGoal != null && c.Sponsorship.TotalCollected.Amount >= c.Sponsorship.FinancialGoal.Amount;
                    targetType = "Sponsorship";
                }
                else if (c.EmergencyCaseId.HasValue && c.EmergencyCase != null)
                {
                    title = c.EmergencyCase.Title;
                    imagePath = c.EmergencyCase.ImagePath;
                    targetGoal = c.EmergencyCase.RequiredAmount.Amount;
                    targetCollected = c.EmergencyCase.CollectedAmount.Amount;
                    isCompleted = c.EmergencyCase.IsCompleted;
                    targetType = "EmergencyCase";
                }

                return new CartItemDto
                {
                    Id = c.Id,
                    DonorId = c.DonorId,
                    SponsorshipId = c.SponsorshipId,
                    EmergencyCaseId = c.EmergencyCaseId,
                    DonationAmount = c.DonationAmount.Amount,
                    Currency = c.DonationAmount.Currency,
                    CreatedAt = c.CreatedAt,
                    TargetType = targetType,
                    Title = title,
                    ImagePath = imagePath,
                    TargetGoalAmount = targetGoal,
                    TargetCollectedAmount = targetCollected,
                    IsCompleted = isCompleted
                };
            }).ToList();

            return Result<IReadOnlyList<CartItemDto>>.Success(dtos, "تم استرجاع عناصر السلة بنجاح.");
        }
    }
}
