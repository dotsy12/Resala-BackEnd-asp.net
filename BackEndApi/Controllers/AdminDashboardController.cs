using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Dashboard;
using BackEnd.Application.Features.Admin.Dashboard.Queries.GetDonationTypeStats;
using BackEnd.Application.Features.Admin.Dashboard.Queries.GetEmergencyCaseStats;
using BackEnd.Application.Features.Admin.Dashboard.Queries.GetMonthlyDonationTrend;
using BackEnd.Application.Features.Admin.Dashboard.Queries.GetOverview;
using BackEnd.Application.Features.Admin.Dashboard.Queries.GetUserStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackEndApi.Controllers
{
    [ApiController]
    [Route("api/v1/admin/dashboard")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Admin Dashboard — إحصائيات لوحة التحكم للمسؤول")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// [Admin] ملخص إحصائيات لوحة التحكم
        /// </summary>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(Result<DashboardOverviewDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] ملخص إحصائيات لوحة التحكم",
            Description = "يرجع إحصائيات عامة تشمل إجمالي التبرعات، المتبرعين، الحالات، والكفالات مع رسم بياني شهري.",
            OperationId = "AdminDashboard_GetOverview",
            Tags = new[] { "Admin Dashboard" }
        )]
        public async Task<ActionResult<Result<DashboardOverviewDto>>> GetOverview(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDashboardOverviewQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] إحصائيات أنواع التبرعات
        /// </summary>
        [HttpGet("donation-types")]
        [ProducesResponseType(typeof(Result<List<DonationTypeStatsDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] إحصائيات أنواع التبرعات",
            Description = "يرجع توزيع التبرعات حسب النوع (كفالة، حالة حرجة، تبرع عام) مع العدد والإجمالي.",
            OperationId = "AdminDashboard_GetDonationTypes",
            Tags = new[] { "Admin Dashboard" }
        )]
        public async Task<ActionResult<Result<List<DonationTypeStatsDto>>>> GetDonationTypeStats(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDonationTypeStatsQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] إحصائيات حالات الطوارئ
        /// </summary>
        [HttpGet("emergency-cases")]
        [ProducesResponseType(typeof(Result<EmergencyCaseStatsDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] إحصائيات حالات الطوارئ",
            Description = "يرجع إحصائيات مفصلة عن حالات الطوارئ (النشطة، المكتملة، الحرجة) وأعلى الحالات تبرعاً.",
            OperationId = "AdminDashboard_GetEmergencyCases",
            Tags = new[] { "Admin Dashboard" }
        )]
        public async Task<ActionResult<Result<EmergencyCaseStatsDto>>> GetEmergencyCaseStats(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEmergencyCaseStatsQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] رسم بياني للتبرعات الشهرية (آخر 12 شهر)
        /// </summary>
        [HttpGet("monthly-donations")]
        [ProducesResponseType(typeof(Result<List<MonthlyDonationTrendDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] رسم بياني للتبرعات الشهرية",
            Description = "يرجع اتجاه التبرعات خلال آخر 12 شهر مع الإجمالي وعدد الحركات.",
            OperationId = "AdminDashboard_GetMonthlyDonations",
            Tags = new[] { "Admin Dashboard" }
        )]
        public async Task<ActionResult<Result<List<MonthlyDonationTrendDto>>>> GetMonthlyDonations(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMonthlyDonationTrendQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] إحصائيات المستخدمين
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(Result<UserStatsDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] إحصائيات المستخدمين",
            Description = "يرجع إحصائيات المستخدمين، المتبرعين، النشطين، والجدد، مع توزيع حسب الأدوار.",
            OperationId = "AdminDashboard_GetUsers",
            Tags = new[] { "Admin Dashboard" }
        )]
        public async Task<ActionResult<Result<UserStatsDto>>> GetUserStats(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetUserStatsQuery(), ct);
            return Ok(result);
        }
    }
}
