using System;

namespace BackEnd.Application.Dtos.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int DonorId { get; set; }
        public int? SponsorshipId { get; set; }
        public int? EmergencyCaseId { get; set; }
        public decimal DonationAmount { get; set; }
        public string Currency { get; set; } = "EGP";
        public DateTime CreatedAt { get; set; }

        // Linked item details
        public string TargetType { get; set; } = null!; // "Sponsorship" or "EmergencyCase"
        public string Title { get; set; } = null!;      // Name/Title
        public string? ImagePath { get; set; }
        public decimal? TargetGoalAmount { get; set; }   // Goal amount
        public decimal? TargetCollectedAmount { get; set; } // Collected amount
        public bool IsCompleted { get; set; }
    }
}
