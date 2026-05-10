using BackEnd.Application.Dtos.Dashboard;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken ct = default);
        Task<List<DonationTypeStatsDto>> GetDonationTypeStatsAsync(CancellationToken ct = default);
        Task<EmergencyCaseStatsDto> GetEmergencyCaseStatsAsync(CancellationToken ct = default);
        Task<List<MonthlyDonationTrendDto>> GetMonthlyDonationTrendAsync(CancellationToken ct = default);
        Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default);
    }
}
