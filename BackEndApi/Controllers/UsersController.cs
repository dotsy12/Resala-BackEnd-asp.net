using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.User;
using BackEnd.Application.Features.Users.Commands.UpdateProfile;
using BackEnd.Application.Features.Users.Commands.UpdateProfileImage;
using BackEnd.Application.Features.Users.Queries.GetProfile;
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

        /// <summary>جلب بيانات الملف الشخصي</summary>
        /// <remarks>
        /// يرجع بيانات المتبرع الحالي مع إحصائيات الاشتراكات والمدفوعات.
        /// </remarks>
        [HttpGet("profile")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] جلب الملف الشخصي",
            Tags = new[] { "Users — Profile" })]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
            => Ok(await _mediator.Send(new GetProfileQuery(GetUserId()), ct));

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
