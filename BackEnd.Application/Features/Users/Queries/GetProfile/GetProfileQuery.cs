using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
using BackEnd.Application.Features.Users.Queries.GetEmergencyContributions;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Users.Queries.GetProfile
{
    public record GetProfileQuery(string UserId) : IRequest<Result<UserProfileDto>>;

    public class GetProfileHandler : IRequestHandler<GetProfileQuery, Result<UserProfileDto>>
    {
        private readonly IDonorRepository _donorRepo;
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly IPaymentRequestRepository _paymentRepo;

        public GetProfileHandler(
            IDonorRepository donorRepo,
            ISponsorshipSubscriptionRepository subRepo,
            IPaymentRequestRepository paymentRepo)
        {
            _donorRepo = donorRepo;
            _subRepo = subRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<Result<UserProfileDto>> Handle(GetProfileQuery request, CancellationToken ct)
        {
            var donor = await _donorRepo.GetByUserIdAsync(request.UserId, ct);
            if (donor == null || donor.User == null)
                return Result<UserProfileDto>.Failure("المستخدم غير موجود.", ErrorType.NotFound);

            var subs = await _subRepo.GetByDonorIdAsync(donor.Id, ct);
            var activeSubsCount = subs.Count(s => s.Status == SubscriptionStatus.Active);
            
            var allPayments = await _paymentRepo.GetByDonorIdAsync(donor.Id, ct);
            var verifiedPayments = allPayments.Where(p => p.Status == PaymentStatus.Verified).ToList();
            
            var subscriptionPaymentsCount = verifiedPayments.Count(p => p.TargetType == PaymentTargetType.Subscription);

            var emergencyPayments = verifiedPayments.Where(p => p.TargetType == PaymentTargetType.EmergencyCase).ToList();
            var totalEmergencyAmount = emergencyPayments.Sum(p => p.Amount.Amount);
            var emergencyCasesCount = emergencyPayments.Select(p => p.EmergencyCaseId).Distinct().Count();

            var recentContributions = emergencyPayments
                .OrderByDescending(p => p.CreatedOn)
                .Take(5)
                .Select(p => new EmergencyContributionDto(
                    PaymentId: p.Id,
                    EmergencyCaseId: p.EmergencyCaseId ?? 0,
                    EmergencyCaseTitle: p.EmergencyCase?.Title ?? "حالة طوارئ",
                    Amount: p.Amount.Amount,
                    Status: p.Status.ToString(),
                    Method: p.Method.ToString(),
                    RejectionReason: p.RejectionReason,
                    CreatedAt: p.CreatedOn,
                    VerifiedAt: p.VerifiedAt
                )).ToList();

            var dto = new UserProfileDto(
                Id: donor.User.Id,
                FullName: $"{donor.User.FirstName} {donor.User.LastName}".Trim(),
                Email: donor.User.Email ?? "",
                Phone: donor.User.PhoneNumber ?? "",
                Address: donor.User.Address,
                Governorate: donor.User.Governorate,
                ProfileImageUrl: donor.User.ProfileImagePath,
                CreatedAt: donor.User.CreatedOn,
                ActiveSubscriptionsCount: activeSubsCount,
                TotalPaymentsCount: verifiedPayments.Count,
                TotalEmergencyDonationsAmount: totalEmergencyAmount,
                EmergencyCasesCount: emergencyCasesCount,
                EmergencyContributions: recentContributions
            );

            return Result<UserProfileDto>.Success(dto);
        }
    }
}
