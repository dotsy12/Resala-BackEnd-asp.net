using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Users.Queries.GetEmergencyContributions
{
    public class GetDonorEmergencyContributionsHandler
        : IRequestHandler<GetDonorEmergencyContributionsQuery, Result<IReadOnlyList<EmergencyContributionDto>>>
    {
        private readonly IPaymentRequestRepository _paymentRepo;

        public GetDonorEmergencyContributionsHandler(IPaymentRequestRepository paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        public async Task<Result<IReadOnlyList<EmergencyContributionDto>>> Handle(
            GetDonorEmergencyContributionsQuery request, CancellationToken ct)
        {
            var payments = await _paymentRepo.GetEmergencyDonationsByDonorIdAsync(request.DonorId, ct);

            var result = payments.Select(p => new EmergencyContributionDto(
                PaymentId: p.Id,
                EmergencyCaseId: p.EmergencyCaseId ?? 0,
                EmergencyCaseTitle: p.EmergencyCase?.Title ?? "N/A",
                Amount: p.Amount.Amount,
                Method: p.Method.ToString(),
                Status: p.Status.ToString(),
                RejectionReason: p.RejectionReason,
                CreatedAt: p.CreatedOn,
                VerifiedAt: p.VerifiedAt
            )).ToList().AsReadOnly();

            return Result<IReadOnlyList<EmergencyContributionDto>>.Success(result);
        }
    }
}