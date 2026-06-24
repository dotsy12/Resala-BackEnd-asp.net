using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
using BackEnd.Application.Dtos.Donor;
using BackEnd.Application.Features.Users.Commands.UpdateProfile;
using BackEnd.Application.Features.Users.Commands.UpdateProfileImage;
using BackEnd.Application.Features.Users.Queries.GetProfile;
using BackEnd.Application.Features.Users.Queries.GetEmergencyContributions;
using BackEnd.Application.Features.Users.Queries.GetEmergencyCaseContributionDetail;
using BackEnd.Application.Features.Users.Queries.GetUserSponsorshipsList;
using BackEnd.Application.Features.Users.Queries.GetUserSponsorshipDetail;
using BackEnd.Application.Features.Donors.Queries.GetDonorStatistics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace BackEnd.Api.Controllers
{
    /// <summary>
    /// إدارة حسابات المتبرعين — User Profile Management
    /// </summary>
    [Route("api/v1/users")]
    [ApiController]
    [Produces("application/json")]
    [SwaggerTag("Users — الملف الشخصي للمتبرعين")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator) => _mediator = mediator;

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        private int GetDonorId() => int.TryParse(User.FindFirst("donorId")?.Value, out var id) ? id : 0;

        /// <summary>جلب بيانات الملف الشخصي</summary>
        /// <remarks>
        /// يرجع بيانات المتبرع الحالي مع إحصائيات الاشتراكات والمدفوعات وإجمالي تبرعات حالات الطوارئ.
        /// </remarks>
        [HttpGet("profile")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] جلب الملف الشخصي",
            Tags = new[] { "Users — Profile" })]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
            => Ok(await _mediator.Send(new GetProfileQuery(GetUserId()), ct));

        /// <summary>جلب إحصائيات لوحة التحكم للمتبرع</summary>
        /// <remarks>
        /// يرجع إحصائيات عامة للمتبرع المسجل الدخول حالياً، مثل عدد الاشتراكات النشطة،
        /// المبالغ الشهرية الملتزم بها، والمجموع الكلي للمدفوعات التي تم التحقق منها.
        /// </remarks>
        [HttpGet("statistics")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<DonorStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "[Donor] إحصائيات لوحة التحكم",
            Description = "يجلب إحصائيات مفصلة للمتبرع الحالي تشمل التبرعات النشطة والمبالغ المدفوعة.",
            OperationId = "GetDonorStatistics",
            Tags = new[] { "Users — Profile" })]
        public async Task<IActionResult> GetStatistics(CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
            {
                return Unauthorized(Result<DonorStatisticsDto>.Failure("Donor not found.", ErrorType.Unauthorized));
            }

            var result = await _mediator.Send(new GetDonorStatisticsQuery(donorId), ct);
            if (!result.IsSuccess)
            {
                if (result.ErrorType == ErrorType.NotFound)
                    return NotFound(result);
                if (result.ErrorType == ErrorType.Unauthorized)
                    return Unauthorized(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>جلب قائمة اشتراكات الكفالة الخاصة بالمتبرع</summary>
        [HttpGet("sponsorships")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SubscriptionSummaryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "[Donor] قائمة اشتراكات الكفالة",
            Description = "يجلب قائمة بجميع اشتراكات الكفالة الحالية والسابقة للمتبرع.",
            OperationId = "GetUserSponsorships",
            Tags = new[] { "Users — Profile" })]
        public async Task<IActionResult> GetUserSponsorships(CancellationToken ct)
            => Ok(await _mediator.Send(new GetUserSponsorshipsListQuery(GetUserId()), ct));

        /// <summary>جلب تفاصيل اشتراك كفالة معين للمتبرع</summary>
        [HttpGet("sponsorships/{subscriptionId:int}")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<UserSponsorshipDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "[Donor] تفاصيل اشتراك الكفالة",
            Description = "يجلب التفاصيل الكاملة وتاريخ المدفوعات لاشتراك كفالة معين للمتبرع بعد التحقق من ملكيته.",
            OperationId = "GetUserSponsorshipDetail",
            Tags = new[] { "Users — Profile" })]
        public async Task<IActionResult> GetUserSponsorshipDetail(int subscriptionId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetUserSponsorshipDetailQuery(GetUserId(), subscriptionId), ct);
            if (!result.IsSuccess)
            {
                if (result.ErrorType == ErrorType.NotFound)
                    return NotFound(result);
                if (result.ErrorType == ErrorType.Forbidden)
                    return StatusCode(StatusCodes.Status403Forbidden, result);
                if (result.ErrorType == ErrorType.Unauthorized)
                    return Unauthorized(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>جلب تاريخ تبرعات حالات الطوارئ</summary>
        [HttpGet("emergency-contributions")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmergencyContributionDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] تاريخ تبرعات حالات الطوارئ",
            Tags = new[] { "Users — Profile" })]
        public async Task<IActionResult> GetEmergencyContributions(CancellationToken ct)
            => Ok(await _mediator.Send(new GetDonorEmergencyContributionsQuery(GetDonorId()), ct));

        /// <summary>جلب تفاصيل تبرعات المستخدم لحالة طوارئ معينة</summary>
        [HttpGet("emergency-cases/{emergencyCaseId:int}")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<UserEmergencyCaseDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Donor] تفاصيل تبرعات الحالة",
            Tags = new[] { "Users — Profile" })]
        public async Task<IActionResult> GetEmergencyCaseContributionDetail(int emergencyCaseId, CancellationToken ct)
            => Ok(await _mediator.Send(new GetEmergencyCaseContributionDetailQuery(GetUserId(), emergencyCaseId), ct));

        /// <summary>تحديث بيانات الملف الشخصي</summary>
        [HttpPut("profile")]
        [Authorize(Roles = "Donor")]
        [SwaggerOperation(
            Summary = "[Donor] تحديث الملف الشخصي",
            Tags = new[] { "Users — Profile" })]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken ct)
            => Ok(await _mediator.Send(new UpdateProfileCommand(GetUserId(), dto), ct));

        /// <summary>تحديث صورة الملف الشخصي</summary>
        [HttpPut("profile/image")]
        [Authorize(Roles = "Donor")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "[Donor] تحديث صورة البروفايل",
            Tags = new[] { "Users — Profile" })]
        public async Task<IActionResult> UpdateProfileImage(IFormFile file, CancellationToken ct)
            => Ok(await _mediator.Send(new UpdateProfileImageCommand(GetUserId(), file), ct));
    }
}
