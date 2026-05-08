using BackEnd.Application.Dtos.FinancialAnalysis;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Entities.Sponsorship;
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
    public class FinancialAnalysisRepository : IFinancialAnalysisRepository
    {
        private readonly ApplicationDbContext _db;

        public FinancialAnalysisRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GlobalDashboardAnalyticsDto> GetGlobalDashboardAnalyticsAsync(CancellationToken ct = default)
        {
            var totalUsers = await _db.Users.CountAsync(ct);
            var totalDonors = await _db.Donors.CountAsync(ct);
            
            var activeSubscriptions = await _db.SponsorshipSubscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .Select(s => new { s.DonorId, s.Amount, s.PaymentCycle })
                .ToListAsync(ct);

            var totalActiveSponsors = activeSubscriptions.Select(s => s.DonorId).Distinct().Count();
            
            // Expected monthly revenue = Sum(Amount / PaymentCycle_in_months)
            var totalMonthlyExpectedRevenue = activeSubscriptions.Sum(s => s.Amount.Amount / (int)s.PaymentCycle);

            var verifiedSubscriptionPayments = await _db.PaymentRequests
                .Where(p => p.TargetType == PaymentTargetType.Subscription && p.Status == PaymentStatus.Verified)
                .SumAsync(p => p.Amount.Amount, ct);

            // To calculate "Remaining Revenue" system-wide, we need all subscriptions' expected vs paid.
            // This could be heavy, so we calculate it efficiently in memory for all subscriptions
            var allSubscriptions = await _db.SponsorshipSubscriptions
                .Select(s => new { 
                    s.Id, s.Amount, s.PaymentCycle, s.StartDate, s.Status 
                })
                .ToListAsync(ct);
                
            var subPaymentsGrouped = await _db.PaymentRequests
                .Where(p => p.TargetType == PaymentTargetType.Subscription && p.Status == PaymentStatus.Verified && p.TargetId != null)
                .GroupBy(p => p.TargetId)
                .Select(g => new { SubId = g.Key!.Value, Paid = g.Sum(p => p.Amount.Amount) })
                .ToDictionaryAsync(g => g.SubId, g => g.Paid, ct);

            decimal totalRemainingRevenue = 0;
            foreach (var sub in allSubscriptions)
            {
                int monthsPassed = ((DateTime.UtcNow.Year - sub.StartDate.Year) * 12) + DateTime.UtcNow.Month - sub.StartDate.Month;
                if (monthsPassed < 0) monthsPassed = 0;
                int expectedPaymentsCount = (monthsPassed / (int)sub.PaymentCycle) + 1;
                decimal expectedAmount = expectedPaymentsCount * sub.Amount.Amount;
                
                decimal paidAmount = subPaymentsGrouped.ContainsKey(sub.Id) ? subPaymentsGrouped[sub.Id] : 0;
                
                decimal remaining = expectedAmount - paidAmount;
                if (remaining > 0)
                {
                    totalRemainingRevenue += remaining;
                }
            }

            var emergencyPayments = await _db.PaymentRequests
                .Where(p => p.TargetType == PaymentTargetType.EmergencyCase && p.Status == PaymentStatus.Verified)
                .SumAsync(p => p.Amount.Amount, ct);

            var totalInKindQuantity = await _db.InKindDonations.SumAsync(i => i.Quantity, ct);

            var usersWithDelayed = await _db.SponsorshipSubscriptions
                .Where(s => s.Status == SubscriptionStatus.Active && s.NextPaymentDate < DateTime.UtcNow)
                .Select(s => s.DonorId)
                .Distinct()
                .CountAsync(ct);

            // Most active donors
            var donorPayments = await _db.PaymentRequests
                .Where(p => p.Status == PaymentStatus.Verified)
                .GroupBy(p => p.DonorId)
                .Select(g => new { DonorId = g.Key, Total = g.Sum(p => p.Amount.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync(ct);

            var mostActiveDonorIds = donorPayments.Select(d => d.DonorId).ToList();
            var donorsDetails = await _db.Donors
                .Where(d => mostActiveDonorIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.FullName.FirstName + " " + d.FullName.LastName, ct);

            var mostActiveDonorsList = donorPayments.Select(d => new ActiveUserDto
            {
                DonorId = d.DonorId,
                FullName = donorsDetails.ContainsKey(d.DonorId) ? donorsDetails[d.DonorId] : "Unknown",
                TotalAmount = d.Total
            }).ToList();

            var mostActiveSponsors = new List<ActiveUserDto>(); // Could be based on subscriptions amount
            var sponsorPayments = await _db.PaymentRequests
                .Where(p => p.TargetType == PaymentTargetType.Subscription && p.Status == PaymentStatus.Verified)
                .GroupBy(p => p.DonorId)
                .Select(g => new { DonorId = g.Key, Total = g.Sum(p => p.Amount.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync(ct);
                
            var mostActiveSponsorIds = sponsorPayments.Select(d => d.DonorId).ToList();
            var sponsorDonorsDetails = await _db.Donors
                .Where(d => mostActiveSponsorIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.FullName.FirstName + " " + d.FullName.LastName, ct);

            mostActiveSponsors = sponsorPayments.Select(d => new ActiveUserDto
            {
                DonorId = d.DonorId,
                FullName = sponsorDonorsDetails.ContainsKey(d.DonorId) ? sponsorDonorsDetails[d.DonorId] : "Unknown",
                TotalAmount = d.Total
            }).ToList();


            return new GlobalDashboardAnalyticsDto
            {
                TotalUsers = totalUsers,
                TotalActiveSponsors = totalActiveSponsors,
                TotalMonthlyExpectedRevenue = totalMonthlyExpectedRevenue,
                TotalCollectedRevenue = verifiedSubscriptionPayments,
                TotalRemainingRevenue = totalRemainingRevenue,
                UsersWithDelayedPayments = usersWithDelayed,
                TotalEmergencyDonationsAmount = emergencyPayments,
                TotalInKindDonationsQuantity = totalInKindQuantity,
                MostActiveDonors = mostActiveDonorsList,
                MostActiveSponsors = mostActiveSponsors
            };
        }

        public async Task<UserFinancialAnalysisDto?> GetUserFinancialAnalysisByIdAsync(int donorId, CancellationToken ct = default)
        {
            var donor = await _db.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == donorId, ct);

            if (donor == null) return null;

            return await BuildUserFinancialAnalysisDtoAsync(donor, ct);
        }

        public async Task<(IReadOnlyList<UserFinancialAnalysisDto> Items, int TotalCount)> GetUsersFinancialAnalysisAsync(
            string? search, 
            string? statusFilter, 
            bool? activeSubscriptions,
            bool? delayedUsers,
            bool? emergencyDonors,
            bool? inKindDonors,
            string? sortBy, 
            int pageNumber, 
            int pageSize, 
            CancellationToken ct = default)
        {
            var query = _db.Donors.Include(d => d.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => 
                    d.FullName.FirstName.Contains(search) || 
                    d.FullName.LastName.Contains(search) ||
                    d.User!.Email.Contains(search) ||
                    d.User!.PhoneNumber.Contains(search));
            }

            if (activeSubscriptions == true)
            {
                query = query.Where(d => _db.SponsorshipSubscriptions.Any(s => s.DonorId == d.Id && s.Status == SubscriptionStatus.Active));
            }

            if (delayedUsers == true)
            {
                query = query.Where(d => _db.SponsorshipSubscriptions.Any(s => s.DonorId == d.Id && s.Status == SubscriptionStatus.Active && s.NextPaymentDate < DateTime.UtcNow));
            }

            if (emergencyDonors == true)
            {
                query = query.Where(d => _db.PaymentRequests.Any(p => p.DonorId == d.Id && p.TargetType == PaymentTargetType.EmergencyCase && p.Status == PaymentStatus.Verified));
            }

            if (inKindDonors == true)
            {
                query = query.Where(d => _db.InKindDonations.Any(i => i.DonorId == d.Id));
            }

            var donors = await query.ToListAsync(ct); // Retrieve filtered donors first
            var results = new List<UserFinancialAnalysisDto>();

            foreach (var donor in donors)
            {
                var dto = await BuildUserFinancialAnalysisDtoAsync(donor, ct);
                
                bool include = true;
                if (!string.IsNullOrWhiteSpace(statusFilter))
                {
                    if (dto.Summary.FinancialStatusName != statusFilter) include = false;
                }
                
                if (include) results.Add(dto);
            }

            // Apply in-memory sorting
            IEnumerable<UserFinancialAnalysisDto> sortedResults = results;
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                sortedResults = sortBy.ToLower() switch
                {
                    "totalpaid" => sortedResults.OrderByDescending(r => r.Summary.TotalPaidAmount),
                    "remainingamount" => sortedResults.OrderByDescending(r => r.Summary.TotalRemainingAmount),
                    "lastpaymentdate" => sortedResults.OrderByDescending(r => r.Summary.LastPaymentDate),
                    "username" => sortedResults.OrderBy(r => r.FullName),
                    "sponsorships" => sortedResults.OrderByDescending(r => r.Summary.TotalSponsorshipsCount),
                    _ => sortedResults.OrderByDescending(r => r.Summary.TotalPaidAmount)
                };
            }
            else
            {
                sortedResults = sortedResults.OrderByDescending(r => r.Summary.TotalPaidAmount);
            }

            var totalCount = sortedResults.Count();
            var pagedItems = sortedResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return (pagedItems, totalCount);
        }

        private async Task<UserFinancialAnalysisDto> BuildUserFinancialAnalysisDtoAsync(Donor donor, CancellationToken ct)
        {
            var dto = new UserFinancialAnalysisDto
            {
                DonorId = donor.Id,
                UserId = donor.UserId,
                FullName = $"{donor.FullName.FirstName} {donor.FullName.LastName}",
                Email = donor.Email?.Value ?? donor.User?.Email ?? "",
                PhoneNumber = donor.PhoneNumber?.Value ?? donor.User?.PhoneNumber ?? "",
                AccountCreationDate = donor.User?.CreatedOn ?? donor.CreatedOn,
                RoleName = "Donor"
            };

            // Fetch Sponsorships
            var subscriptions = await _db.SponsorshipSubscriptions
                .Include(s => s.Sponsorship)
                .Where(s => s.DonorId == donor.Id)
                .ToListAsync(ct);

            var subIds = subscriptions.Select(s => (int?)s.Id).ToList();
            
            var allSubPayments = await _db.PaymentRequests
                .Where(p => p.DonorId == donor.Id && p.TargetType == PaymentTargetType.Subscription && p.Status == PaymentStatus.Verified)
                .ToListAsync(ct);

            decimal totalRemainingAll = 0;
            decimal totalPaidAll = 0;
            int lateSubscriptions = 0;

            foreach (var sub in subscriptions)
            {
                var subPayments = allSubPayments.Where(p => p.TargetId == sub.Id).ToList();
                decimal paidAmount = subPayments.Sum(p => p.Amount.Amount);
                
                int monthsPassed = ((DateTime.UtcNow.Year - sub.StartDate.Year) * 12) + DateTime.UtcNow.Month - sub.StartDate.Month;
                if (monthsPassed < 0) monthsPassed = 0;
                int expectedPaymentsCount = (monthsPassed / (int)sub.PaymentCycle) + 1;
                decimal expectedAmount = expectedPaymentsCount * sub.Amount.Amount;
                decimal remaining = expectedAmount - paidAmount;
                if (remaining < 0) remaining = 0;

                totalRemainingAll += remaining;
                totalPaidAll += paidAmount;

                if (sub.IsLate(0)) lateSubscriptions++;

                var sponsorshipDto = new SponsorshipAnalysisDto
                {
                    SponsorshipId = sub.SponsorshipId,
                    SponsorshipName = sub.Sponsorship?.Name ?? "Unknown",
                    MonthlySubscriptionAmount = sub.Amount.Amount,
                    StartDate = sub.StartDate,
                    EndDate = sub.CancelledAt,
                    IsActive = sub.Status == SubscriptionStatus.Active,
                    TotalExpectedAmount = expectedAmount,
                    TotalPaidAmount = paidAmount,
                    RemainingAmount = remaining,
                    PaymentHistory = GeneratePaymentHistory(sub, subPayments)
                };

                dto.Sponsorships.Add(sponsorshipDto);
            }

            // In-Kind Donations
            var inKinds = await _db.InKindDonations
                .Where(i => i.DonorId == donor.Id)
                .ToListAsync(ct);

            dto.InKindDonations = inKinds.Select(i => new InKindDonationAnalysisDto
            {
                DonationId = i.Id,
                DonationName = i.DonationTypeName,
                Quantity = i.Quantity,
                DonationDate = i.CreatedOn,
                DonationCategory = null
            }).ToList();

            // Emergency Case Donations
            var emergencyPayments = await _db.PaymentRequests
                .Include(p => p.EmergencyCase)
                .Where(p => p.DonorId == donor.Id && p.TargetType == PaymentTargetType.EmergencyCase && p.Status == PaymentStatus.Verified)
                .ToListAsync(ct);

            dto.EmergencyDonations = emergencyPayments.Select(p => new EmergencyDonationAnalysisDto
            {
                EmergencyCaseId = p.TargetId ?? 0,
                EmergencyCaseTitle = p.EmergencyCase?.Title ?? "Unknown",
                DonationAmount = p.Amount.Amount,
                DonationDate = p.CreatedOn,
                PaymentStatus = "Verified"
            }).ToList();

            // Last payment
            var allPayments = new List<DateTime>();
            allPayments.AddRange(allSubPayments.Select(p => p.CreatedOn));
            allPayments.AddRange(emergencyPayments.Select(p => p.CreatedOn));
            var lastPaymentDate = allPayments.Any() ? allPayments.Max() : (DateTime?)null;

            // Determine status
            string statusName = "منتظم";
            string statusColor = "Green";
            int statusPriority = 1;

            if (totalRemainingAll > 0)
            {
                statusName = "باقي عليه فلوس";
                statusColor = "Orange";
                statusPriority = 2;
            }
            
            if (lateSubscriptions > 0)
            {
                statusName = "متأخر";
                statusColor = "Red";
                statusPriority = 3;
            }

            decimal totalExpectedAll = dto.Sponsorships.Sum(s => s.TotalExpectedAmount);
            decimal regularityPercentage = totalExpectedAll > 0 ? (totalPaidAll / totalExpectedAll) * 100 : 100;
            if (regularityPercentage > 100) regularityPercentage = 100;

            dto.Summary = new UserSummaryAnalyticsDto
            {
                TotalSponsorshipsCount = subscriptions.Count,
                ActiveSponsorshipsCount = subscriptions.Count(s => s.Status == SubscriptionStatus.Active),
                TotalPaidAmount = totalPaidAll,
                TotalRemainingAmount = totalRemainingAll,
                TotalEmergencyDonations = emergencyPayments.Sum(p => p.Amount.Amount),
                TotalInKindDonationsCount = inKinds.Count,
                TotalInKindDonationsQuantity = inKinds.Sum(i => i.Quantity),
                LastPaymentDate = lastPaymentDate,
                FinancialStatusName = statusName,
                FinancialStatusColor = statusColor,
                FinancialStatusPriority = statusPriority,
                PaymentRegularityPercentage = Math.Round(regularityPercentage, 2)
            };

            return dto;
        }

        private List<PaymentHistoryDto> GeneratePaymentHistory(SponsorshipSubscription sub, List<PaymentRequest> payments)
        {
            var history = new List<PaymentHistoryDto>();
            var currentDate = DateTime.UtcNow;
            
            // Generate months from StartDate up to CurrentDate
            var iterateDate = new DateTime(sub.StartDate.Year, sub.StartDate.Month, 1);
            var cycleMonths = (int)sub.PaymentCycle;

            while (iterateDate <= currentDate)
            {
                var cycleEndDate = iterateDate.AddMonths(cycleMonths).AddDays(-1);
                
                // Check if any payment falls in this cycle, or just keep it simple: assign a payment to each cycle sequentially
                // Actually, the simplest is checking if there's enough total paid to cover this cycle
                
                // Let's do it simple:
                history.Add(new PaymentHistoryDto
                {
                    Month = iterateDate.ToString("MMMM"),
                    Year = iterateDate.Year,
                    Status = "Expected" 
                });
                
                iterateDate = iterateDate.AddMonths(cycleMonths);
            }

            // Fill statuses based on total paid vs expected
            decimal remainingPaid = payments.Sum(p => p.Amount.Amount);
            decimal cycleAmount = sub.Amount.Amount;

            foreach (var record in history)
            {
                if (remainingPaid >= cycleAmount)
                {
                    record.Status = "Paid";
                    remainingPaid -= cycleAmount;
                }
                else if (remainingPaid > 0)
                {
                    record.Status = "Partial";
                    remainingPaid = 0;
                }
                else
                {
                    record.Status = "Missing";
                }
            }

            return history;
        }
    }
}
