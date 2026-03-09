using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Events;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Payment
{
    public sealed class PaymentRequest : BaseEntity<int>, IAggregateRoot
    {
        public int? SubscriptionId { get; private set; }
        public int? GeneralDonationId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public PaymentMethod Method { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string? ReceiptImagePath { get; private set; }
        public BranchPaymentDetails? BranchDetails { get; private set; }
        public RepresentativeDetails? RepresentativeInfo { get; private set; }
        public int? VerifiedByStaffId { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public string? RejectionReason { get; private set; }

        private PaymentRequest() { }

        // ── Vodafone Cash / InstaPay ──────────────────────────
        public static PaymentRequest CreateElectronic(
            int? subscriptionId, int? generalDonationId,
            Money amount, PaymentMethod method, string receiptImagePath)
        {
            ValidateReference(subscriptionId, generalDonationId);
            if (method is not (PaymentMethod.VodafoneCash or PaymentMethod.InstaPay))
                throw new InvalidPaymentRequestException(
                    "Use this factory for VodafoneCash or InstaPay only.");
            if (string.IsNullOrWhiteSpace(receiptImagePath))
                throw new InvalidPaymentRequestException(
                    "Receipt image is required.");

            return new PaymentRequest
            {
                SubscriptionId = subscriptionId,
                GeneralDonationId = generalDonationId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Pending,
                ReceiptImagePath = receiptImagePath,
                CreatedOn = DateTime.UtcNow
            };
        }

        // ── Branch ────────────────────────────────────────────
        public static PaymentRequest CreateBranch(
            int? subscriptionId, int? generalDonationId,
            Money amount, BranchPaymentDetails branchDetails)
        {
            ValidateReference(subscriptionId, generalDonationId);
            return new PaymentRequest
            {
                SubscriptionId = subscriptionId,
                GeneralDonationId = generalDonationId,
                Amount = amount,
                Method = PaymentMethod.Branch,
                Status = PaymentStatus.Pending,
                BranchDetails = branchDetails
                    ?? throw new ArgumentNullException(nameof(branchDetails)),
                CreatedOn = DateTime.UtcNow
            };
        }

        // ── Representative ────────────────────────────────────
        public static PaymentRequest CreateRepresentative(
            int? subscriptionId, int? generalDonationId,
            Money amount, RepresentativeDetails repDetails)
        {
            ValidateReference(subscriptionId, generalDonationId);
            return new PaymentRequest
            {
                SubscriptionId = subscriptionId,
                GeneralDonationId = generalDonationId,
                Amount = amount,
                Method = PaymentMethod.Representative,
                Status = PaymentStatus.Pending,
                RepresentativeInfo = repDetails
                    ?? throw new ArgumentNullException(nameof(repDetails)),
                CreatedOn = DateTime.UtcNow
            };
        }

        // ── Business Methods ──────────────────────────────────
        public void Verify(int staffId)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidPaymentRequestException(
                    $"Cannot verify payment with status '{Status}'.");

            Status = PaymentStatus.Verified;
            VerifiedByStaffId = staffId;
            VerifiedAt = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;

            AddDomainEvent(new PaymentVerifiedEvent(
                Id, SubscriptionId, GeneralDonationId,
                Amount.Amount, staffId));
        }

        public void Reject(int staffId, string reason)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidPaymentRequestException(
                    $"Cannot reject payment with status '{Status}'.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required.");

            Status = PaymentStatus.Rejected;
            VerifiedByStaffId = staffId;
            RejectionReason = reason.Trim();
            UpdatedOn = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidPaymentRequestException(
                    "Only pending payments can be cancelled.");
            Status = PaymentStatus.Cancelled;
            UpdatedOn = DateTime.UtcNow;
        }

        private static void ValidateReference(
            int? subscriptionId, int? generalDonationId)
        {
            if (subscriptionId.HasValue == generalDonationId.HasValue)
                throw new InvalidPaymentRequestException(
                    "Link to subscription XOR generalDonation — not both or neither.");
        }
    }
}