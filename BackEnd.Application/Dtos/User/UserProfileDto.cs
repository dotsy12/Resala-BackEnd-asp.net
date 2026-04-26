namespace BackEnd.Application.Dtos.User
{
    public record UserProfileDto(
        string Id,
        string FullName,
        string Email,
        string Phone,
        string? Address,
        string? Governorate,
        string? ProfileImageUrl,
        DateTime CreatedAt,
        int ActiveSubscriptionsCount,
        int TotalPaymentsCount,
        decimal TotalEmergencyDonationsAmount,
        int EmergencyCasesCount,
        List<EmergencyContributionDto> EmergencyContributions
    );

    public record EmergencyContributionDto(
        int PaymentId,
        int EmergencyCaseId,
        string CaseTitle,
        decimal Amount,
        string PaymentStatus,
        string PaymentMethod,
        DateTime PaymentDate
    );

    public record UserEmergencyCaseDetailsDto(
        int EmergencyCaseId,
        string CaseTitle,
        string CaseStatus,
        decimal TotalUserContribution,
        int DonationsCount,
        decimal CaseTotalGoal,
        decimal CaseCollectedAmount,
        decimal RemainingAmount,
        List<UserEmergencyDonationItemDto> Donations
    );

    public record UserEmergencyDonationItemDto(
        int PaymentId,
        decimal Amount,
        string PaymentMethod,
        string PaymentStatus,
        DateTime Date
    );

    public record UpdateProfileDto(
        string FullName,
        string Phone,
        string? Address,
        string? Governorate
    );
}
