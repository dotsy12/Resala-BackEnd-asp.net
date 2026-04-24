namespace BackEnd.Application.Dtos.Subscription
{
    public record SubscriptionDto(
        int Id,
        int DonorId,
        string DonorName,
        int SponsorshipId,
        string SponsorshipName,
        decimal Amount,
        string PaymentCycle,
        string Status,
        DateTime StartDate,
        DateTime NextPaymentDate,
        DateTime CreatedOn,
        List<PaymentRequestSummaryDto> RecentPayments
    );

    public record PaymentRequestSummaryDto(
        int Id,
        string Method,
        string Status,
        decimal Amount,
        string? ReceiptImageUrl,
        string? ReceiptImagePublicId,
        string? SenderPhoneNumber,
        string? ContactName,
        string? ContactPhone,
        DateTime? ScheduledDate,
        string? RejectionReason,
        DateTime CreatedOn
    );

    public record AppointmentSlotDto(
        int Id,
        DateTime SlotDate,
        string OpenFrom,
        string OpenTo,
        int MaxCapacity,
        int BookedCount,
        int AvailableSpots,
        string? Notes
    );

    public record DeliveryAreaDto(int Id, string Name);

    public record UpdateSubscriptionDto(decimal? NewAmount, string? NewPaymentCycle);
}