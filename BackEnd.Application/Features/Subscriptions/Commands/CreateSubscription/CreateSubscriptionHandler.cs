// CreateSubscriptionHandler.cs
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Sponsorship;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionHandler
        : IRequestHandler<CreateSubscriptionCommand, Result<SubscriptionDto>>
    {
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly ISponsorshipRepository _sponsorshipRepo;
        private readonly IDonorRepository _donorRepo;
        private readonly ILogger<CreateSubscriptionHandler> _logger;

        public CreateSubscriptionHandler(
            ISponsorshipSubscriptionRepository subRepo,
            ISponsorshipRepository sponsorshipRepo,
            IDonorRepository donorRepo,
            ILogger<CreateSubscriptionHandler> logger)
        {
            _subRepo = subRepo;
            _sponsorshipRepo = sponsorshipRepo;
            _donorRepo = donorRepo;
            _logger = logger;
        }

        public async Task<Result<SubscriptionDto>> Handle(
            CreateSubscriptionCommand request, CancellationToken ct)
        {
            _logger.LogInformation(
                "إنشاء اشتراك — DonorId:{DonorId} SponsorshipId:{SponsorshipId}",
                request.DonorId, request.Dto.SponsorshipId);

            var donor = await _donorRepo.GetByIdAsync(request.DonorId, ct);
            if (donor is null)
                return Result<SubscriptionDto>.Failure(
                    "المتبرع غير موجود.", ErrorType.NotFound);

            var sponsorship = await _sponsorshipRepo.GetByIdAsync(request.Dto.SponsorshipId, ct);
            if (sponsorship is null)
                return Result<SubscriptionDto>.Failure(
                    "برنامج الكفالة غير موجود.", ErrorType.NotFound);

            if (!sponsorship.IsActive)
                return Result<SubscriptionDto>.Failure(
                    "برنامج الكفالة غير متاح حالياً.", ErrorType.BadRequest);

            // منع الاشتراك المكرر في نفس البرنامج
            var existing = await _subRepo.GetActiveByDonorAndSponsorshipAsync(
                request.DonorId, request.Dto.SponsorshipId, ct);

            if (existing is not null)
                return Result<SubscriptionDto>.Failure(
                    "أنت مشترك بالفعل في هذا البرنامج.", ErrorType.Conflict);

            var subscription = SponsorshipSubscription.Create(
                donorId: request.DonorId,
                sponsorshipId: request.Dto.SponsorshipId,
                sponsorship: sponsorship,
                amount: new Money(request.Dto.Amount),
                cycle: request.Dto.PaymentCycle
            );

            await _subRepo.AddAsync(subscription, ct);
            await _subRepo.SaveChangesAsync(ct);

            _logger.LogInformation("تم إنشاء الاشتراك: Id={Id}", subscription.Id);

            return Result<SubscriptionDto>.Success(new SubscriptionDto(
                Id: subscription.Id,
                DonorId: subscription.DonorId,
                DonorName: donor.FullName.FullName,
                SponsorshipId: subscription.SponsorshipId,
                SponsorshipName: sponsorship.Name,
                Amount: subscription.Amount.Amount,
                PaymentCycle: subscription.PaymentCycle.ToString(),
                Status: subscription.Status.ToString(),
                StartDate: subscription.StartDate,
                NextPaymentDate: subscription.NextPaymentDate,
                CreatedOn: subscription.CreatedOn,
                RecentPayments: []
            ), "تم إنشاء الاشتراك بنجاح. يمكنك الآن تقديم طلب الدفع.");
        }
    }
}