using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

public class GetSubscriptionByIdHandler
    : IRequestHandler<GetSubscriptionByIdQuery, Result<SubscriptionDto>>
{
    private readonly ISponsorshipSubscriptionRepository _repo;

    public GetSubscriptionByIdHandler(ISponsorshipSubscriptionRepository repo)
        => _repo = repo;

    public async Task<Result<SubscriptionDto>> Handle(
        GetSubscriptionByIdQuery request, CancellationToken ct)
    {
        var sub = await _repo.GetByIdAsync(request.SubscriptionId, ct);
        if (sub is null)
            return Result<SubscriptionDto>.Failure(
                "الاشتراك غير موجود.", ErrorType.NotFound);

        // Donor يشوف اشتراكاته فقط
        if (request.RequesterRole == "Donor" && sub.DonorId != request.RequesterId)
            return Result<SubscriptionDto>.Failure("غير مصرّح.", ErrorType.Forbidden);

        return Result<SubscriptionDto>.Success(new SubscriptionDto(
            Id: sub.Id,
            DonorId: sub.DonorId,
            DonorName: sub.Donor?.FullName.FullName ?? "",
            SponsorshipId: sub.SponsorshipId,
            SponsorshipName: sub.Sponsorship?.Name ?? "",
            Amount: sub.Amount.Amount,
            PaymentCycle: sub.PaymentCycle.ToString(),
            Status: sub.Status.ToString(),
            StartDate: sub.StartDate,
            NextPaymentDate: sub.NextPaymentDate,
            CreatedOn: sub.CreatedOn,
            RecentPayments: []
        ));
    }
}