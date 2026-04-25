using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
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
            
            int totalPaymentsCount = 0;
            foreach (var sub in subs)
            {
                var payments = await _paymentRepo.GetBySubscriptionIdAsync(sub.Id, ct);
                totalPaymentsCount += payments.Count(p => p.Status == PaymentStatus.Verified);
            }

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
                TotalPaymentsCount: totalPaymentsCount
            );

            return Result<UserProfileDto>.Success(dto);
        }
    }
}
