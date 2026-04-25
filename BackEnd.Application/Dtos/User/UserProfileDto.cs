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
        int TotalPaymentsCount
    );

    public record UpdateProfileDto(
        string FullName,
        string Phone,
        string? Address,
        string? Governorate
    );
}
