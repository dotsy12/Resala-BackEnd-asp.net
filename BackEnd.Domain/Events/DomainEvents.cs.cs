// Domain/Events/DomainEvents.cs
using BackEnd.Domain.Enums;
using BackEnd.Domain.Interfaces;

namespace BackEnd.Domain.Events
{
    public sealed record DonorRegisteredEvent(
        int DonorId, string Email)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record PaymentVerifiedEvent(
        int PaymentRequestId,
        int? SubscriptionId,
        int? GeneralDonationId,
        int? EmergencyCaseId,
        decimal Amount,
        int VerifiedByStaffId)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record PaymentRejectedEvent(
        int PaymentRequestId, int DonorId, string Reason)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record SubscriptionCreatedEvent(
        int SubscriptionId, int DonorId, int SponsorshipId)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record SubscriptionCancelledEvent(
        int SubscriptionId, int DonorId, string? Reason)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record LatePaymentDetectedEvent(
        int SubscriptionId, int DonorId, DateTime DueDate)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record SponsorshipUrgencyChangedEvent(
        int SponsorshipId, UrgencyLevel NewLevel)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }
}