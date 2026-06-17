using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Enums;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _db;

        public DashboardRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        private static DateTime? GetStartDate(DashboardPeriod period)
        {
            return period switch
            {
                DashboardPeriod.LastWeek => DateTime.UtcNow.AddDays(-7),
                DashboardPeriod.LastMonth => DateTime.UtcNow.AddMonths(-1),
                DashboardPeriod.LastSixMonths => DateTime.UtcNow.AddMonths(-6),
                DashboardPeriod.LastYear => DateTime.UtcNow.AddYears(-1),
                DashboardPeriod.AllTime => null,
                _ => DateTime.UtcNow.AddMonths(-1)
            };
        }

        public async Task<DashboardOverviewDto> GetOverviewAsync(DashboardPeriod period, CancellationToken ct = default)
        {
            var startDate = GetStartDate(period);

            var donationsQuery = _db.PaymentRequests.Where(p => p.Status == PaymentStatus.Verified);
            if (startDate.HasValue)
                donationsQuery = donationsQuery.Where(p => p.CreatedOn >= startDate.Value);

            var totalDonations = await donationsQuery.SumAsync(p => p.Amount.Amount, ct);

            var totalDonors = await donationsQuery
                .Select(p => p.DonorId)
                .Distinct()
                .CountAsync(ct);

            var emergencyCasesQuery = _db.EmergencyCases.AsQueryable();
            if (startDate.HasValue)
                emergencyCasesQuery = emergencyCasesQuery.Where(c => c.CreatedOn >= startDate.Value);
            
            var totalEmergencyCases = await emergencyCasesQuery.CountAsync(ct);
            
            var sponsorshipsQuery = _db.Sponsorships.AsQueryable();
            if (startDate.HasValue)
                sponsorshipsQuery = sponsorshipsQuery.Where(s => s.CreatedOn >= startDate.Value);

            var totalSponsorships = await sponsorshipsQuery.CountAsync(ct);
            
            var totalPaymentRequests = await donationsQuery.CountAsync(ct);

            var monthlyTrend = await GetMonthlyDonationTrendAsync(period, ct);

            return new DashboardOverviewDto
            {
                TotalDonations = totalDonations,
                TotalDonors = totalDonors,
                TotalEmergencyCases = totalEmergencyCases,
                TotalSponsorships = totalSponsorships,
                TotalPaymentRequests = totalPaymentRequests,
                MonthlyDonations = monthlyTrend
            };
        }

        public async Task<List<DonationTypeStatsDto>> GetDonationTypeStatsAsync(DashboardPeriod period, CancellationToken ct = default)
        {
            var startDate = GetStartDate(period);
            var query = _db.PaymentRequests.Where(p => p.Status == PaymentStatus.Verified);

            if (startDate.HasValue)
                query = query.Where(p => p.CreatedOn >= startDate.Value);

            var stats = await query
                .GroupBy(p => p.TargetType)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount.Amount)
                })
                .ToListAsync(ct);

            return stats.Select(s => new DonationTypeStatsDto
            {
                TypeName = MapTargetTypeToName(s.Type),
                Count = s.Count,
                TotalAmount = s.TotalAmount
            }).ToList();
        }

        public async Task<EmergencyCaseStatsDto> GetEmergencyCaseStatsAsync(DashboardPeriod period, CancellationToken ct = default)
        {
            var startDate = GetStartDate(period);
            var casesQuery = _db.EmergencyCases.AsNoTracking();

            if (startDate.HasValue)
                casesQuery = casesQuery.Where(c => c.CreatedOn >= startDate.Value);

            var totalCases = await casesQuery.CountAsync(ct);
            var activeCases = await casesQuery.CountAsync(c => c.IsActive, ct);
            var completedCases = await casesQuery.CountAsync(c => c.CollectedAmount.Amount >= c.RequiredAmount.Amount, ct);
            var criticalCases = await casesQuery.CountAsync(c => c.UrgencyLevel == UrgencyLevel.Critical, ct);
            var totalCollected = await casesQuery.SumAsync(c => (decimal?)c.CollectedAmount.Amount ?? 0, ct);

            var topCases = await casesQuery
                .OrderByDescending(c => c.CollectedAmount.Amount)
                .Take(5)
                .Select(c => new TopEmergencyCaseDto
                {
                    CaseId = c.Id,
                    Title = c.Title,
                    CollectedAmount = c.CollectedAmount.Amount,
                    TargetAmount = c.RequiredAmount.Amount,
                    DonorsCount = _db.PaymentRequests
                        .Count(p => p.EmergencyCaseId == c.Id && p.Status == PaymentStatus.Verified)
                })
                .ToListAsync(ct);

            var stoppedCount = await casesQuery.CountAsync(c => !c.IsActive && c.CollectedAmount.Amount < c.RequiredAmount.Amount, ct);

            var statusDistribution = new List<StatusDistributionDto>
            {
                new() { Status = "نشطة", Count = activeCases },
                new() { Status = "مكتملة", Count = completedCases },
                new() { Status = "متوقفة", Count = stoppedCount }
            };

            return new EmergencyCaseStatsDto
            {
                TotalCases = totalCases,
                ActiveCases = activeCases,
                CompletedCases = completedCases,
                CriticalCases = criticalCases,
                TotalCollectedAmount = totalCollected,
                TopCases = topCases,
                StatusDistribution = statusDistribution
            };
        }

        public async Task<List<MonthlyDonationTrendDto>> GetMonthlyDonationTrendAsync(DashboardPeriod period, CancellationToken ct = default)
        {
            var startDate = GetStartDate(period) ?? DateTime.UtcNow.AddMonths(-11);
            // If AllTime, we still need a start point for the trend, let's default to last 12 months for visual trend if AllTime is selected but trend is requested.
            // Or better, adjust the trend based on period.
            
            int monthsToDisplay = 12;
            if (period == DashboardPeriod.LastWeek) monthsToDisplay = 1; // Not great for monthly trend, but let's see.
            if (period == DashboardPeriod.LastSixMonths) monthsToDisplay = 6;
            if (period == DashboardPeriod.LastYear) monthsToDisplay = 12;
            if (period == DashboardPeriod.AllTime) monthsToDisplay = 24; // Show more for all time?

            var actualStartDate = new DateTime(startDate.Year, startDate.Month, 1);

            var data = await _db.PaymentRequests
                .Where(p => p.Status == PaymentStatus.Verified && p.CreatedOn >= actualStartDate)
                .GroupBy(p => new { p.CreatedOn.Year, p.CreatedOn.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Amount = g.Sum(p => p.Amount.Amount),
                    Count = g.Count()
                })
                .ToListAsync(ct);

            var result = new List<MonthlyDonationTrendDto>();
            for (int i = 0; i < monthsToDisplay; i++)
            {
                var date = actualStartDate.AddMonths(i);
                if (date > DateTime.UtcNow) break;

                var monthData = data.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);
                
                result.Add(new MonthlyDonationTrendDto
                {
                    Month = date.ToString("MMM yyyy"),
                    Amount = monthData?.Amount ?? 0,
                    PaymentsCount = monthData?.Count ?? 0
                });
            }

            return result;
        }

        public async Task<UserStatsDto> GetUserStatsAsync(DashboardPeriod period, CancellationToken ct = default)
        {
            var startDate = GetStartDate(period);
            var usersQuery = _db.Users.AsQueryable();
            var donorsQuery = _db.Donors.AsQueryable();

            if (startDate.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.CreatedOn >= startDate.Value);
                donorsQuery = donorsQuery.Where(d => d.CreatedOn >= startDate.Value);
            }

            var totalUsers = await usersQuery.CountAsync(ct);
            var totalDonors = await donorsQuery.CountAsync(ct);
            var activeUsers = await usersQuery.CountAsync(u => u.EmailConfirmed, ct);
            
            var thisMonth = DateTime.UtcNow.Month;
            var thisYear = DateTime.UtcNow.Year;
            var newUsersThisMonth = await _db.Users.CountAsync(u => u.CreatedOn.Month == thisMonth && u.CreatedOn.Year == thisYear, ct);

            var subQuery = _db.SponsorshipSubscriptions.Where(s => s.Status == SubscriptionStatus.Active);
            if (startDate.HasValue)
                subQuery = subQuery.Where(s => s.CreatedOn >= startDate.Value);

            var subscribedUsers = await subQuery
                .Select(s => s.DonorId)
                .Distinct()
                .CountAsync(ct);

            var roleDistribution = await (from userRole in _db.UserRoles
                                         join role in _db.Roles on userRole.RoleId equals role.Id
                                         join user in _db.Users on userRole.UserId equals user.Id
                                         where !startDate.HasValue || user.CreatedOn >= startDate.Value
                                         group userRole by role.Name into g
                                         select new RoleDistributionDto
                                         {
                                             RoleName = g.Key ?? "Unknown",
                                             Count = g.Count()
                                         }).ToListAsync(ct);

            return new UserStatsDto
            {
                TotalUsers = totalUsers,
                TotalDonors = totalDonors,
                ActiveUsers = activeUsers,
                NewUsersThisMonth = newUsersThisMonth,
                SubscribedUsers = subscribedUsers,
                NonSubscribedUsers = totalUsers - subscribedUsers,
                UsersByRole = roleDistribution
            };
        }

        public async Task<SponsorshipStatsDto> GetSponsorshipStatsAsync(DashboardPeriod period, CancellationToken ct = default)
        {
            var startDate = GetStartDate(period);
            var topSponsorships = await _db.Sponsorships
                .Select(s => new TopSponsorshipDto
                {
                    SponsorshipId = s.Id,
                    Title = s.Name,
                    CollectedAmount = _db.PaymentRequests
                        .Where(p => p.Subscription!.SponsorshipId == s.Id && p.Status == PaymentStatus.Verified && (!startDate.HasValue || p.CreatedOn >= startDate.Value))
                        .Sum(p => p.Amount.Amount),
                    TargetAmount = s.FinancialGoal != null ? s.FinancialGoal.Amount : 0,
                    DonorsCount = _db.PaymentRequests
                        .Where(p => p.Subscription!.SponsorshipId == s.Id && p.Status == PaymentStatus.Verified && (!startDate.HasValue || p.CreatedOn >= startDate.Value))
                        .Select(p => p.DonorId)
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(x => x.CollectedAmount)
                .ToListAsync(ct);

            return new SponsorshipStatsDto
            {
                TopSponsorships = topSponsorships
            };
        }

        private static string MapTargetTypeToName(PaymentTargetType type)
        {
            return type switch
            {
                PaymentTargetType.Subscription => "كفالة",
                PaymentTargetType.EmergencyCase => "حالة حرجة",
                PaymentTargetType.GeneralDonation => "تبرع عام",
                _ => "أخرى"
            };
        }
    }
}