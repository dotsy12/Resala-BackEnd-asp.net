using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Users.Queries.GetEmergencyContributions
{
    public record GetDonorEmergencyContributionsQuery(string UserId) : IRequest<Result<IReadOnlyList<EmergencyContributionDto>>>;

    public class GetDonorEmergencyContributionsHandler : IRequestHandler<GetDonorEmergencyContributionsQuery, Result<IReadOnlyList<EmergencyContributionDto>>>
    {
        private readonly IDonorRepository _donorRepo;
        private readonly IPaymentRequestRepository _paymentRepo;

        public GetDonorEmergencyContributionsHandler(IDonorRepository donorRepo, IPaymentRequestRepository paymentRepo)
        {
            _donorRepo = donorRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<Result<IReadOnlyList<EmergencyContributionDto>>> Handle(GetDonorEmergencyContributionsQuery request, CancellationToken ct)
        {
            var donor = await _donorRepo.GetByUserIdAsync(request.UserId, ct);
            if (donor == null)
                return Result<IReadOnlyList<EmergencyContributionDto>>.Failure("المستخدم غير موجود.", ErrorType.NotFound);

            var payments = await _paymentRepo.GetEmergencyDonationsByDonorIdAsync(donor.Id, ct);

            var result = payments.Select(p => new EmergencyContributionDto(
                PaymentId: p.Id,
                EmergencyCaseId: p.EmergencyCaseId ?? 0,
                CaseTitle: p.EmergencyCase?.Title ?? "حالة طوارئ",
                Amount: p.Amount.Amount,
                PaymentStatus: p.Status.ToString(),
                PaymentMethod: p.Method.ToString(),
                PaymentDate: p.CreatedOn
            )).ToList().AsReadOnly();

            return Result<IReadOnlyList<EmergencyContributionDto>>.Success(result);
        }
    }
}
