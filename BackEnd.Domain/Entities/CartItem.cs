using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.EmergencyCase;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Entities.Sponsorship;
using BackEnd.Domain.ValueObjects;
using System;

namespace BackEnd.Domain.Entities
{
    public sealed class CartItem : BaseEntity<int>
    {
        public int DonorId { get; private set; }
        public Donor? Donor { get; private set; }

        public int? SponsorshipId { get; private set; }
        public BackEnd.Domain.Entities.Sponsorship.Sponsorship? Sponsorship { get; private set; }

        public int? EmergencyCaseId { get; private set; }
        public BackEnd.Domain.Entities.EmergencyCase.EmergencyCase? EmergencyCase { get; private set; }

        public Money DonationAmount { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }

        private CartItem() { }

        public static CartItem Create(int donorId, int? sponsorshipId, int? emergencyCaseId, Money donationAmount)
        {
            if (sponsorshipId == null && emergencyCaseId == null)
                throw new ArgumentException("Either SponsorshipId or EmergencyCaseId must be specified.");

            if (sponsorshipId != null && emergencyCaseId != null)
                throw new ArgumentException("Cannot specify both SponsorshipId and EmergencyCaseId.");

            if (donationAmount == null)
                throw new ArgumentNullException(nameof(donationAmount));

            return new CartItem
            {
                DonorId = donorId,
                SponsorshipId = sponsorshipId,
                EmergencyCaseId = emergencyCaseId,
                DonationAmount = donationAmount,
                CreatedAt = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };
        }

        public void UpdateAmount(Money newAmount)
        {
            DonationAmount = newAmount ?? throw new ArgumentNullException(nameof(newAmount));
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
