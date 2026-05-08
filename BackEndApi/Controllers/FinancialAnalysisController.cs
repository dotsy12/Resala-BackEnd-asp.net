using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.FinancialAnalysis;
using BackEnd.Application.Features.FinancialAnalysis.Queries.GetGlobalDashboardAnalytics;
using BackEnd.Application.Features.FinancialAnalysis.Queries.GetUserFinancialAnalysisById;
using BackEnd.Application.Features.FinancialAnalysis.Queries.GetUsersFinancialAnalysis;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.API.Controllers
{
    [ApiController]
    [Route("api/v1/staff")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,Reception,Staff")]
    [SwaggerTag("Financial Analysis Dashboard — لوحة التحليل المالي ونشاط المستخدمين")]
    public class FinancialAnalysisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinancialAnalysisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ═══════════════════════════════════════════════════
        //  1. DASHBOARD ANALYTICS
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Staff/Admin/Reception] ملخص لوحة التحكم المالي العامة
        /// </summary>
        /// <remarks>
        /// Retrieves high-level global financial metrics, total users, expected vs. collected revenue, and lists of the most active donors/sponsors.
        /// </remarks>
        [HttpGet("dashboard/financial-summary")]
        [ProducesResponseType(typeof(ApiResponse<GlobalDashboardAnalyticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Staff] ملخص لوحة التحكم للتحليل المالي",
            Description = "إحصائيات شاملة للنظام مالياً ونشاط المتبرعين.",
            OperationId = "FinancialAnalysis_GetGlobalSummary",
            Tags = new[] { "Financial Analysis Dashboard" }
        )]
        public async Task<IActionResult> GetGlobalDashboardSummary(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetGlobalDashboardAnalyticsQuery(), ct);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════
        //  2. USER FINANCIAL LISTING
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Staff/Admin/Reception] التحليل المالي لكل المستخدمين مع فلترة وبحث
        /// </summary>
        /// <remarks>
        /// Retrieves a paginated, searchable, and filterable list of all donors with their summarized financial status.
        /// Supports search by name, email, or phone. Advanced filters: status, active subscriptions, delayed users, etc.
        /// </remarks>
        [HttpGet("users/financial-analysis")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<UserFinancialAnalysisDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Staff] قائمة التحليل المالي للمستخدمين",
            Description = "قائمة بجميع المتبرعين مع ملخص حالتهم المالية والبحث المتقدم.",
            OperationId = "FinancialAnalysis_GetUsersList",
            Tags = new[] { "Financial Analysis Dashboard" }
        )]
        public async Task<IActionResult> GetUsersFinancialAnalysis([FromQuery] GetUsersFinancialAnalysisQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════
        //  3. SINGLE USER DEEP DIVE
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Staff/Admin/Reception] التحليل المالي الشامل لمستخدم (متبرع) محدد
        /// </summary>
        /// <remarks>
        /// Retrieves the complete deep-dive financial profile of a specific donor including all sponsorships with month-by-month payment history, emergency donations, and summary.
        /// </remarks>
        [HttpGet("users/{donorId:int}/financial-analysis")]
        [ProducesResponseType(typeof(ApiResponse<UserFinancialAnalysisDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Staff] التحليل المالي التفصيلي لمستخدم محدد",
            Description = "عرض تفصيلي لاشتراكات وتبرعات وتاريخ دفع المتبرع لكل شهر.",
            OperationId = "FinancialAnalysis_GetUserDetail",
            Tags = new[] { "Financial Analysis Dashboard" }
        )]
        public async Task<IActionResult> GetUserFinancialAnalysisById(int donorId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetUserFinancialAnalysisByIdQuery(donorId), ct);
            if (!result.IsSuccess)
                return NotFound(result);
                
            return Ok(result);
        }
    }
}
