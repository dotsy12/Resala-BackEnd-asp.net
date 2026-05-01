namespace BackEnd.Application.Features.Users.Queries.GetEmergencyContributions
{
    public record EmergencyContributionDto(
        int PaymentId,
        int EmergencyCaseId,
        string EmergencyCaseTitle,
        decimal Amount,
        string Method,
        string Status,
        string? RejectionReason,
        DateTime CreatedAt,
        DateTime? VerifiedAt
    );
}