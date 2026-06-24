using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Enums;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Users.Queries.GetUserSponsorshipDetail
{
    public record GetUserSponsorshipDetailQuery(string UserId, int SubscriptionId) : IRequest<Result<UserSponsorshipDetailDto>>;

    public class GetUserSponsorshipDetailHandler : IRequestHandler<GetUserSponsorshipDetailQuery, Result<UserSponsorshipDetailDto>>
    {
        private readonly IDonorRepository _donorRepo;
        private readonly ISponsorshipSubscriptionRepository _subscriptionRepo;
        private readonly IPaymentRequestRepository _paymentRepo;

        public GetUserSponsorshipDetailHandler(
            IDonorRepository donorRepo,
            ISponsorshipSubscriptionRepository subscriptionRepo,
            IPaymentRequestRepository paymentRepo)
        {
            _donorRepo = donorRepo;
            _subscriptionRepo = subscriptionRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<Result<UserSponsorshipDetailDto>> Handle(GetUserSponsorshipDetailQuery request, CancellationToken ct)
        {
            // 1. Verify the donor exists
            var donor = await _donorRepo.GetByUserIdAsync(request.UserId, ct);
            if (donor == null)
            {
                return Result<UserSponsorshipDetailDto>.Failure("المستخدم غير موجود.", ErrorType.NotFound);
            }

            // 2. Fetch the specific subscription
            var subscription = await _subscriptionRepo.GetByIdAsync(request.SubscriptionId, ct);
            if (subscription == null)
            {
                return Result<UserSponsorshipDetailDto>.Failure("الاشتراك غير موجود.", ErrorType.NotFound);
            }

            // 3. Authorization check: Ensure the subscription belongs to the logged-in donor
            if (subscription.DonorId != donor.Id)
            {
                return Result<UserSponsorshipDetailDto>.Failure("غير مصرح بالوصول لتفاصيل هذا الاشتراك.", ErrorType.Forbidden);
            }

            // 4. Fetch the payment history for this subscription
            var payments = await _paymentRepo.GetBySubscriptionIdAsync(request.SubscriptionId, ct);

            // 5. Compute metrics
            decimal totalPaidAmount = payments
                .Where(p => p.Status == PaymentStatus.Verified)
                .Sum(p => p.Amount.Amount);

            var paymentList = payments.Select(p => new SponsorshipPaymentItemDto(
                PaymentId: p.Id,
                Amount: p.Amount.Amount,
                PaymentMethod: p.Method.ToString(),
                PaymentStatus: p.Status.ToString(),
                Date: p.CreatedOn,
                VerifiedAt: p.VerifiedAt,
                RejectionReason: p.RejectionReason
            )).ToList();

            var dto = new UserSponsorshipDetailDto(
                SubscriptionId: subscription.Id,
                SponsorshipId: subscription.SponsorshipId,
                SponsorshipName: subscription.Sponsorship?.Name ?? "كفالة",
                SponsorshipDescription: subscription.Sponsorship?.Description ?? "",
                SponsorshipImagePath: subscription.Sponsorship?.ImagePath,
                Amount: subscription.Amount.Amount,
                PaymentCycle: subscription.PaymentCycle.ToString(),
                Status: subscription.Status.ToString(),
                StartDate: subscription.StartDate,
                NextPaymentDate: subscription.NextPaymentDate,
                CancelledAt: subscription.CancelledAt,
                CancelReason: subscription.CancelReason,
                TotalPaidAmount: totalPaidAmount,
                PaymentsCount: payments.Count,
                Payments: paymentList
            );

            return Result<UserSponsorshipDetailDto>.Success(dto);
        }
    }
}
