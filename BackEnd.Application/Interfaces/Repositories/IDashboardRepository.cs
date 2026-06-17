using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Domain.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<DashboardOverviewDto> GetOverviewAsync(DashboardPeriod period, CancellationToken ct = default);
        Task<List<DonationTypeStatsDto>> GetDonationTypeStatsAsync(DashboardPeriod period, CancellationToken ct = default);
        Task<EmergencyCaseStatsDto> GetEmergencyCaseStatsAsync(DashboardPeriod period, CancellationToken ct = default);
        Task<List<MonthlyDonationTrendDto>> GetMonthlyDonationTrendAsync(DashboardPeriod period, CancellationToken ct = default);
        Task<UserStatsDto> GetUserStatsAsync(DashboardPeriod period, CancellationToken ct = default);
        Task<SponsorshipStatsDto> GetSponsorshipStatsAsync(DashboardPeriod period, CancellationToken ct = default);
    }
}
