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

        public async Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken ct = default)
        {
            var totalDonations = await _db.PaymentRequests
                .Where(p => p.Status == PaymentStatus.Verified)
                .SumAsync(p => p.Amount.Amount, ct);

            var totalDonors = await _db.PaymentRequests
                .Where(p => p.Status == PaymentStatus.Verified)
                .Select(p => p.DonorId)
                .Distinct()
                .CountAsync(ct);

            var totalEmergencyCases = await _db.EmergencyCases.CountAsync(ct);
            var totalSponsorships = await _db.Sponsorships.CountAsync(ct);
            var totalPaymentRequests = await _db.PaymentRequests.CountAsync(ct);

            var monthlyTrend = await GetMonthlyDonationTrendAsync(ct);

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

        public async Task<List<DonationTypeStatsDto>> GetDonationTypeStatsAsync(CancellationToken ct = default)
        {
            var stats = await _db.PaymentRequests
                .Where(p => p.Status == PaymentStatus.Verified)
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

        public async Task<EmergencyCaseStatsDto> GetEmergencyCaseStatsAsync(CancellationToken ct = default)
        {
            var allCases = await _db.EmergencyCases
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.IsActive,
                    c.UrgencyLevel,
                    RequiredAmount = c.RequiredAmount.Amount,
                    CollectedAmount = c.CollectedAmount.Amount
                })
                .ToListAsync(ct);

            var totalCases = allCases.Count;
            var activeCases = allCases.Count(c => c.IsActive);
            var completedCases = allCases.Count(c => c.CollectedAmount >= c.RequiredAmount);
            var criticalCases = allCases.Count(c => c.UrgencyLevel == UrgencyLevel.Critical);
            var totalCollected = allCases.Sum(c => c.CollectedAmount);

            var topCases = allCases
                .OrderByDescending(c => c.CollectedAmount)
                .Take(5)
                .Select(c => new TopEmergencyCaseDto
                {
                    CaseId = c.Id,
                    Title = c.Title,
                    CollectedAmount = c.CollectedAmount,
                    TargetAmount = c.RequiredAmount,
                    DonorsCount = _db.PaymentRequests.Count(p => p.EmergencyCaseId == c.Id && p.Status == PaymentStatus.Verified)
                })
                .ToList();

            var statusDistribution = new List<StatusDistributionDto>
            {
                new() { Status = "نشطة", Count = activeCases },
                new() { Status = "مكتملة", Count = completedCases },
                new() { Status = "متوقفة", Count = allCases.Count(c => !c.IsActive && c.CollectedAmount < c.RequiredAmount) }
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

        public async Task<List<MonthlyDonationTrendDto>> GetMonthlyDonationTrendAsync(CancellationToken ct = default)
        {
            var startDate = DateTime.UtcNow.AddMonths(-11);
            startDate = new DateTime(startDate.Year, startDate.Month, 1);

            var data = await _db.PaymentRequests
                .Where(p => p.Status == PaymentStatus.Verified && p.CreatedOn >= startDate)
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
            for (int i = 0; i < 12; i++)
            {
                var date = startDate.AddMonths(i);
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

        public async Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default)
        {
            var totalUsers = await _db.Users.CountAsync(ct);
            var totalDonors = await _db.Donors.CountAsync(ct);
            var activeUsers = await _db.Users.CountAsync(u => u.EmailConfirmed, ct);
            
            var thisMonth = DateTime.UtcNow.Month;
            var thisYear = DateTime.UtcNow.Year;
            var newUsersThisMonth = await _db.Users.CountAsync(u => u.CreatedOn.Month == thisMonth && u.CreatedOn.Year == thisYear, ct);

            // Group by Role
            // This is a bit tricky with ASP.NET Identity, but let's assume we can join Users and UserRoles
            var roleDistribution = await (from userRole in _db.UserRoles
                                         join role in _db.Roles on userRole.RoleId equals role.Id
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
                UsersByRole = roleDistribution
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
