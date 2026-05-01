using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
using BackEnd.Application.Features.Users.Commands.UpdateProfile;
using BackEnd.Application.Features.Users.Commands.UpdateProfileImage;
using BackEnd.Application.Features.Users.Queries.GetProfile;
using BackEnd.Application.Features.Users.Queries.GetEmergencyContributions;
using BackEnd.Application.Features.Users.Queries.GetEmergencyCaseContributionDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
