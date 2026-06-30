using System;
using System.Collections.Generic;

namespace BackEnd.Application.Dtos.User
{
    public record SubscriptionSummaryDto(
        int Id,
        int SponsorshipId,
        string SponsorshipName,
        string SponsorshipCategory,
        decimal Amount,
        string PaymentCycle,
        string Status,
        DateTime StartDate,
        DateTime NextPaymentDate,
        DateTime CreatedOn,
        string? SponsorshipImageUrl,
        string? SponsorshipIcon
    );

    public record UserSponsorshipDetailDto(
        int SubscriptionId,
        int SponsorshipId,
        string SponsorshipName,
        string SponsorshipDescription,
        string? SponsorshipImagePath,
        decimal Amount,
        string PaymentCycle,
        string Status,
        DateTime StartDate,
        DateTime NextPaymentDate,
        DateTime? CancelledAt,
        string? CancelReason,
        decimal TotalPaidAmount,
        int PaymentsCount,
        List<SponsorshipPaymentItemDto> Payments
    );

    public record SponsorshipPaymentItemDto(
        int PaymentId,
        decimal Amount,
        string PaymentMethod,
        string PaymentStatus,
        DateTime Date,
        DateTime? VerifiedAt,
        string? RejectionReason
    );
}
