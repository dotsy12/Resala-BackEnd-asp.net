using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.Entities.Payment
{
    public sealed class InKindDonation : BaseEntity<int>
    {
        public int DonorId { get; private set; }
        public string DonationTypeName { get; private set; } = null!;
        public int Quantity { get; private set; }
        public string? Description { get; private set; }
        public int RecordedByStaffId { get; private set; }
        public DateTime RecordedAt { get; private set; }

        public Donor? Donor { get; private set; }
        public StaffMember? RecordedBy { get; private set; }

        private InKindDonation() { }
        public void Update(string donationTypeName, int quantity, string? description)
        {
            if (quantity <= 0)
                throw new ArgumentException("Invalid quantity");

            if (string.IsNullOrWhiteSpace(donationTypeName))
                throw new ArgumentException("Donation type required");

            DonationTypeName = donationTypeName.Trim();
            Quantity = quantity;
            Description = description?.Trim();
        }
        public static InKindDonation Create(
            int donorId, string donationTypeName,
            int quantity, string? description, int recordedByStaffId)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be > 0.", nameof(quantity));
            if (string.IsNullOrWhiteSpace(donationTypeName))
                throw new ArgumentException("Donation type name is required.");

            return new InKindDonation
            {
                DonorId = donorId,
                DonationTypeName = donationTypeName.Trim(),
                Quantity = quantity,
                Description = description?.Trim(),
                RecordedByStaffId = recordedByStaffId,
                RecordedAt = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}