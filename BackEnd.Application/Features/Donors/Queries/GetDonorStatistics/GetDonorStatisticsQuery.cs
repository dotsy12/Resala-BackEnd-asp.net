using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Donor;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Enums;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Donors.Queries.GetDonorStatistics
{
    public record GetDonorStatisticsQuery(int DonorId) : IRequest<Result<DonorStatisticsDto>>;

    public class GetDonorStatisticsHandler : IRequestHandler<GetDonorStatisticsQuery, Result<DonorStatisticsDto>>
    {
        private readonly IDonorRepository _donorRepo;
        private readonly ISponsorshipSubscriptionRepository _subscriptionRepo;
        private readonly IPaymentRequestRepository _paymentRepo;

        public GetDonorStatisticsHandler(
            IDonorRepository donorRepo,
            ISponsorshipSubscriptionRepository subscriptionRepo,
            IPaymentRequestRepository paymentRepo)
        {
            _donorRepo = donorRepo;
            _subscriptionRepo = subscriptionRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<Result<DonorStatisticsDto>> Handle(GetDonorStatisticsQuery request, CancellationToken cancellationToken)
        {
            // 1. Verify the donor exists
            var donor = await _donorRepo.GetByIdAsync(request.DonorId, cancellationToken);
            if (donor == null)
            {
                return Result<DonorStatisticsDto>.Failure("Donor not found.", ErrorType.NotFound);
            }

            // 2. Fetch subscriptions and payments
            var subscriptions = await _subscriptionRepo.GetByDonorIdAsync(request.DonorId, cancellationToken);
            var payments = await _paymentRepo.GetByDonorIdAsync(request.DonorId, cancellationToken);

            // 3. Compute statistics
            int totalSponsorships = subscriptions.Count;
            int activeSponsorships = subscriptions.Count(s => s.Status == SubscriptionStatus.Active);
            int expiredSponsorships = subscriptions.Count(s => s.Status == SubscriptionStatus.Expired || s.Status == SubscriptionStatus.Cancelled);

            decimal totalMonthlyAmount = subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .Sum(s => s.Amount.Amount / Math.Max(1, (int)s.PaymentCycle));

            decimal totalPaidAmount = payments
                .Where(p => p.Status == PaymentStatus.Verified)
                .Sum(p => p.Amount.Amount);

            var statisticsDto = new DonorStatisticsDto(
                TotalSponsorships: totalSponsorships,
                ActiveSponsorships: activeSponsorships,
                ExpiredSponsorships: expiredSponsorships,
                TotalMonthlyAmount: totalMonthlyAmount,
                TotalPaidAmount: totalPaidAmount
            );

            return Result<DonorStatisticsDto>.Success(statisticsDto);
        }
    }
}
