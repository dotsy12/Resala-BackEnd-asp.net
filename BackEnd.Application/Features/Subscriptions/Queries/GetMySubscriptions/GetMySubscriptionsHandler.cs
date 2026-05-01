// GetMySubscriptionsQuery.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetMySubscriptions
{
    public class GetMySubscriptionsHandler
        : IRequestHandler<GetMySubscriptionsQuery, Result<IReadOnlyList<SubscriptionDto>>>
    {
        private readonly ISponsorshipSubscriptionRepository _repo;
        private readonly IPaymentRequestRepository _paymentRepo;

        public GetMySubscriptionsHandler(
            ISponsorshipSubscriptionRepository repo,
            IPaymentRequestRepository paymentRepo)
        { _repo = repo; _paymentRepo = paymentRepo; }

        public async Task<Result<IReadOnlyList<SubscriptionDto>>> Handle(
            GetMySubscriptionsQuery request, CancellationToken ct)
        {
            var subs = await _repo.GetByDonorIdAsync(request.DonorId, ct);
            var payments = await _paymentRepo.GetByDonorIdAsync(request.DonorId, ct);

            var result = subs.Select(s => new SubscriptionDto(
                Id: s.Id,
                DonorId: s.DonorId,
                DonorName: s.Donor?.FullName.FullName ?? "",
                SponsorshipId: s.SponsorshipId,
                SponsorshipName: s.Sponsorship?.Name ?? "",
                Amount: s.Amount.Amount,
                PaymentCycle: s.PaymentCycle.ToString(),
                Status: s.Status.ToString(),
                StartDate: s.StartDate,
                NextPaymentDate: s.NextPaymentDate,
                CreatedOn: s.CreatedOn,
                RecentPayments: payments
                    .Where(p => p.SubscriptionId == s.Id)
                    .Select(p => new PaymentRequestSummaryDto(
                        Id: p.Id,
                        SubscriptionId: p.SubscriptionId,
                        EmergencyCaseId: p.EmergencyCaseId,
                        EmergencyCaseTitle: p.EmergencyCase?.Title,
                        UserName: null,
                        Phone: null,
                        Method: p.Method.ToString(),
                        Status: p.Status.ToString(),
                        Amount: p.Amount.Amount,
                        ReceiptImageUrl: p.ReceiptImageUrl,
                        ReceiptImagePublicId: p.ReceiptImagePublicId,
                        SenderPhoneNumber: p.SenderPhoneNumber,
                        ContactName: p.RepresentativeInfo?.ContactName,
                        ContactPhone: p.RepresentativeInfo?.ContactPhone ?? p.BranchDetails?.ContactNumber,
                        Address: p.RepresentativeInfo?.Address,
                        RepresentativeNotes: p.RepresentativeInfo?.Notes,
                        DeliveryAreaId: p.RepresentativeInfo?.DeliveryAreaId,
                        DeliveryAreaName: p.RepresentativeInfo?.DeliveryAreaName,
                        ScheduledDate: p.BranchDetails?.ScheduledDate,
                        RejectionReason: p.RejectionReason,
                        CreatedOn: p.CreatedOn
                    )).ToList()
            )).ToList().AsReadOnly();

            return Result<IReadOnlyList<SubscriptionDto>>.Success(result);
        }
    }
}