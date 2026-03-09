// Domain/ValueObjects/ValueObjects.cs
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.ValueObjects
{
    // ── BranchPaymentDetails ─────────────────────────────────
    public sealed class BranchPaymentDetails : IEquatable<BranchPaymentDetails>
    {
        public string DonorName { get; private set; } = null!;
        public string Address { get; private set; } = null!;
        public string ContactNumber { get; private set; } = null!;
        public DateTime ScheduledDate { get; private set; }

        private BranchPaymentDetails() { }

        public BranchPaymentDetails(
            string donorName, string address,
            string contactNumber, DateTime scheduledDate)
        {
            if (string.IsNullOrWhiteSpace(donorName))
                throw new ArgumentException("Donor name is required.");
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address is required.");
            if (scheduledDate.ToUniversalTime() <= DateTime.UtcNow)
                throw new InvalidPaymentRequestException(
                    "Scheduled date must be in the future.");

            DonorName = donorName.Trim();
            Address = address.Trim();
            ContactNumber = contactNumber.Trim();
            ScheduledDate = scheduledDate.ToUniversalTime();
        }

        public bool Equals(BranchPaymentDetails? other) =>
            other is not null &&
            DonorName == other.DonorName && ScheduledDate == other.ScheduledDate;

        public override bool Equals(object? obj) => Equals(obj as BranchPaymentDetails);
        public override int GetHashCode() =>
            HashCode.Combine(DonorName, ScheduledDate);
    }
}