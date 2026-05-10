using System.Collections.Generic;

namespace BackEnd.Application.Dtos.Dashboard
{
    public class DashboardOverviewDto
    {
        public decimal TotalDonations { get; set; }
        public int TotalDonors { get; set; }
        public int TotalEmergencyCases { get; set; }
        public int TotalSponsorships { get; set; }
        public int TotalPaymentRequests { get; set; }
        public List<MonthlyDonationTrendDto> MonthlyDonations { get; set; } = new();
    }

    public class MonthlyDonationTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int PaymentsCount { get; set; }
    }

    public class DonationTypeStatsDto
    {
        public string TypeName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class EmergencyCaseStatsDto
    {
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
        public int CompletedCases { get; set; }
        public int CriticalCases { get; set; }
        public decimal TotalCollectedAmount { get; set; }
        public List<TopEmergencyCaseDto> TopCases { get; set; } = new();
        public List<StatusDistributionDto> StatusDistribution { get; set; } = new();
    }

    public class TopEmergencyCaseDto
    {
        public int CaseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal CollectedAmount { get; set; }
        public decimal TargetAmount { get; set; }
        public int DonorsCount { get; set; }
    }

    public class StatusDistributionDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalDonors { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public List<RoleDistributionDto> UsersByRole { get; set; } = new();
    }

    public class RoleDistributionDto
    {
        public string RoleName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
