using System;
using System.Collections.Generic;

namespace BackEnd.Application.Dtos.FinancialAnalysis
{
    public class UserFinancialAnalysisDto
    {
        public int DonorId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime AccountCreationDate { get; set; }
        public string RoleName { get; set; } = "Donor"; // Assuming they are donors mostly

        public List<SponsorshipAnalysisDto> Sponsorships { get; set; } = new();
        public List<InKindDonationAnalysisDto> InKindDonations { get; set; } = new();
        public List<EmergencyDonationAnalysisDto> EmergencyDonations { get; set; } = new();
        public UserSummaryAnalyticsDto Summary { get; set; } = new();
    }

    public class SponsorshipAnalysisDto
    {
        public int SponsorshipId { get; set; }
        public string SponsorshipName { get; set; } = string.Empty;
        public decimal MonthlySubscriptionAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalExpectedAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public List<PaymentHistoryDto> PaymentHistory { get; set; } = new();
    }

    public class PaymentHistoryDto
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Status { get; set; } = string.Empty; // "Paid", "Missing", "Delayed"
    }

    public class InKindDonationAnalysisDto
    {
        public int DonationId { get; set; }
        public string DonationName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime DonationDate { get; set; }
        public string? DonationCategory { get; set; }
    }

    public class EmergencyDonationAnalysisDto
    {
        public int EmergencyCaseId { get; set; }
        public string EmergencyCaseTitle { get; set; } = string.Empty;
        public decimal DonationAmount { get; set; }
        public DateTime DonationDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class UserSummaryAnalyticsDto
    {
        public int TotalSponsorshipsCount { get; set; }
        public int ActiveSponsorshipsCount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalRemainingAmount { get; set; }
        public decimal TotalEmergencyDonations { get; set; }
        public int TotalInKindDonationsCount { get; set; }
        public int TotalInKindDonationsQuantity { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        
        public string FinancialStatusName { get; set; } = string.Empty;
        public string FinancialStatusColor { get; set; } = string.Empty;
        public int FinancialStatusPriority { get; set; }
        
        public decimal PaymentRegularityPercentage { get; set; }
    }

    public class GlobalDashboardAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalActiveSponsors { get; set; }
        public decimal TotalMonthlyExpectedRevenue { get; set; }
        public decimal TotalCollectedRevenue { get; set; }
        public decimal TotalRemainingRevenue { get; set; }
        public int UsersWithDelayedPayments { get; set; }
        
        public List<ActiveUserDto> MostActiveDonors { get; set; } = new();
        public List<ActiveUserDto> MostActiveSponsors { get; set; } = new();
        
        public decimal TotalEmergencyDonationsAmount { get; set; }
        public int TotalInKindDonationsQuantity { get; set; }
    }

    public class ActiveUserDto
    {
        public int DonorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
