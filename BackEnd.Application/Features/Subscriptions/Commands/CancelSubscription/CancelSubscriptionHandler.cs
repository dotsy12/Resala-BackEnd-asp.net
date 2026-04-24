// CancelSubscriptionCommand.cs + Handler + Validator
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.CancelSubscription
{
    public class CancelSubscriptionHandler
        : IRequestHandler<CancelSubscriptionCommand, Result<string>>
    {
        private readonly ISponsorshipSubscriptionRepository _repo;
        private readonly ILogger<CancelSubscriptionHandler> _logger;

        public CancelSubscriptionHandler(
            ISponsorshipSubscriptionRepository repo,
            ILogger<CancelSubscriptionHandler> logger)
        { _repo = repo; _logger = logger; }

        public async Task<Result<string>> Handle(
            CancelSubscriptionCommand request, CancellationToken ct)
        {
            var sub = await _repo.GetByIdAsync(request.SubscriptionId, ct);
            if (sub is null)
                return Result<string>.Failure("الاشتراك غير موجود.", ErrorType.NotFound);

            if (sub.DonorId != request.DonorId)
                return Result<string>.Failure("غير مصرّح.", ErrorType.Forbidden);

            sub.Cancel(request.Reason);
            _repo.Update(sub);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("تم إلغاء الاشتراك: Id={Id}", sub.Id);
            return Result<string>.Success("تم إلغاء الاشتراك بنجاح.");
        }
    }
}