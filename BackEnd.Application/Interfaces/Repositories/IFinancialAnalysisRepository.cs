using BackEnd.Application.Dtos.FinancialAnalysis;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IFinancialAnalysisRepository
    {
        Task<(IReadOnlyList<UserFinancialAnalysisDto> Items, int TotalCount)> GetUsersFinancialAnalysisAsync(
            string? search, 
            string? statusFilter, 
            bool? activeSubscriptions,
            bool? delayedUsers,
            bool? emergencyDonors,
            bool? inKindDonors,
            string? sortBy, 
            int pageNumber, 
            int pageSize, 
            CancellationToken ct = default);

        Task<UserFinancialAnalysisDto?> GetUserFinancialAnalysisByIdAsync(int donorId, CancellationToken ct = default);

        Task<GlobalDashboardAnalyticsDto> GetGlobalDashboardAnalyticsAsync(CancellationToken ct = default);
    }
}
