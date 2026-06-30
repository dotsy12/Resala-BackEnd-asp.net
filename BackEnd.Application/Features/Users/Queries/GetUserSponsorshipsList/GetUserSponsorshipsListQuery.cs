using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Users.Queries.GetUserSponsorshipsList
{
    public record GetUserSponsorshipsListQuery(string UserId) : IRequest<Result<IReadOnlyList<SubscriptionSummaryDto>>>;

    public class GetUserSponsorshipsListHandler : IRequestHandler<GetUserSponsorshipsListQuery, Result<IReadOnlyList<SubscriptionSummaryDto>>>
    {
        private readonly IDonorRepository _donorRepo;
        private readonly ISponsorshipSubscriptionRepository _subscriptionRepo;

        public GetUserSponsorshipsListHandler(
            IDonorRepository donorRepo,
            ISponsorshipSubscriptionRepository subscriptionRepo)
        {
            _donorRepo = donorRepo;
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<Result<IReadOnlyList<SubscriptionSummaryDto>>> Handle(GetUserSponsorshipsListQuery request, CancellationToken ct)
        {
            var donor = await _donorRepo.GetByUserIdAsync(request.UserId, ct);
            if (donor == null)
            {
                return Result<IReadOnlyList<SubscriptionSummaryDto>>.Failure("المستخدم غير موجود.", ErrorType.NotFound);
            }

            var subscriptions = await _subscriptionRepo.GetByDonorIdAsync(donor.Id, ct);

            var dtos = subscriptions.Select(s => new SubscriptionSummaryDto(
                Id: s.Id,
                SponsorshipId: s.SponsorshipId,
                SponsorshipName: s.Sponsorship?.Name ?? "كفالة",
                SponsorshipCategory: s.Sponsorship?.Category ?? "عام",
                Amount: s.Amount.Amount,
                PaymentCycle: s.PaymentCycle.ToString(),
                Status: s.Status.ToString(),
                StartDate: s.StartDate,
                NextPaymentDate: s.NextPaymentDate,
                CreatedOn: s.CreatedOn,
                SponsorshipImageUrl: s.Sponsorship?.ImagePath,
                SponsorshipIcon: s.Sponsorship?.IconPath
            )).ToList();

            return Result<IReadOnlyList<SubscriptionSummaryDto>>.Success(dtos);
        }
    }
}
