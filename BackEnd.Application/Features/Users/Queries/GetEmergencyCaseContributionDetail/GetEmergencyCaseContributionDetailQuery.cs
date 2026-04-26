using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Users.Queries.GetEmergencyCaseContributionDetail
{
    public record GetEmergencyCaseContributionDetailQuery(string UserId, int EmergencyCaseId) 
        : IRequest<Result<UserEmergencyCaseDetailsDto>>;

    public class GetEmergencyCaseContributionDetailHandler 
        : IRequestHandler<GetEmergencyCaseContributionDetailQuery, Result<UserEmergencyCaseDetailsDto>>
    {
        private readonly IDonorRepository _donorRepo;
        private readonly IPaymentRequestRepository _paymentRepo;

        public GetEmergencyCaseContributionDetailHandler(
            IDonorRepository donorRepo, 
            IPaymentRequestRepository paymentRepo)
        {
            _donorRepo = donorRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<Result<UserEmergencyCaseDetailsDto>> Handle(
            GetEmergencyCaseContributionDetailQuery request, CancellationToken ct)
        {
            var donor = await _donorRepo.GetByUserIdAsync(request.UserId, ct);
            if (donor == null)
                return Result<UserEmergencyCaseDetailsDto>.Failure("المستخدم غير موجود.", ErrorType.NotFound);

            var contributions = await _paymentRepo.GetEmergencyDonationsByDonorAndCaseIdAsync(
                donor.Id, request.EmergencyCaseId, ct);

            if (!contributions.Any())
                return Result<UserEmergencyCaseDetailsDto>.Failure(
                    "لم يتم العثور على تبرعات لهذه الحالة بواسطة هذا المستخدم.", ErrorType.NotFound);

            // First record to get Case Info (since they all share the same EmergencyCase object)
            var first = contributions.First();
            var emergencyCase = first.EmergencyCase;

            if (emergencyCase == null)
                return Result<UserEmergencyCaseDetailsDto>.Failure("بيانات حالة الطوارئ غير مكتملة.", ErrorType.InternalServerError);

            var totalUserContribution = contributions
                .Where(p => p.Status == PaymentStatus.Verified)
                .Sum(p => p.Amount.Amount);

            var donationsList = contributions.Select(p => new UserEmergencyDonationItemDto(
                PaymentId: p.Id,
                Amount: p.Amount.Amount,
                PaymentMethod: p.Method.ToString(),
                PaymentStatus: p.Status.ToString(),
                Date: p.CreatedOn
            )).ToList();

            var dto = new UserEmergencyCaseDetailsDto(
                EmergencyCaseId: emergencyCase.Id,
                CaseTitle: emergencyCase.Title,
                CaseStatus: emergencyCase.IsActive ? "Active" : "Inactive",
                TotalUserContribution: totalUserContribution,
                DonationsCount: contributions.Count,
                CaseTotalGoal: emergencyCase.RequiredAmount.Amount,
                CaseCollectedAmount: emergencyCase.CollectedAmount.Amount,
                RemainingAmount: Math.Max(0, emergencyCase.RequiredAmount.Amount - emergencyCase.CollectedAmount.Amount),
                Donations: donationsList
            );

            return Result<UserEmergencyCaseDetailsDto>.Success(dto);
        }
    }
}
