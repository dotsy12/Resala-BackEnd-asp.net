using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Sponsorship;
using BackEnd.Domain.ValueObjects;
using MediatR;

namespace BackEnd.Application.Features.Admin.DirectOperations.Commands.DirectSubscribe
{
    public class DirectSubscribeHandler : IRequestHandler<DirectSubscribeCommand, Result<int>>
    {
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly ISponsorshipRepository _sponsorshipRepo;
        private readonly IUnitOfWork _unitOfWork;

        public DirectSubscribeHandler(
            ISponsorshipSubscriptionRepository subRepo,
            ISponsorshipRepository sponsorshipRepo,
            IUnitOfWork unitOfWork)
        {
            _subRepo = subRepo;
            _sponsorshipRepo = sponsorshipRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(DirectSubscribeCommand request, CancellationToken ct)
        {
            var sponsorship = await _sponsorshipRepo.GetByIdAsync(request.SponsorshipId, ct);
            if (sponsorship == null)
                return Result<int>.Failure("برنامج الكفالة غير موجود.", ErrorType.NotFound);

            var subscription = SponsorshipSubscription.Create(
                request.DonorId,
                request.SponsorshipId,
                sponsorship,
                new Money(request.Amount),
                request.Cycle
            );

            await _subRepo.AddAsync(subscription, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<int>.Success(subscription.Id, "تم اشتراك المتبرع في البرنامج بنجاح من خلال الفرع.");
        }
    }
}