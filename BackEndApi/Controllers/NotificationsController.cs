using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Notification;
using BackEnd.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using BackEnd.Application.Features.Notifications.Commands.RegisterDeviceToken;
using BackEnd.Application.Features.Notifications.Commands.SendBroadcastNotification;
using BackEnd.Application.Features.Notifications.Queries.GetNotificationHistory;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace BackEnd.Api.Controllers
{
    /// <summary>
    /// إدارة الإشعارات ورموز الأجهزة — Notifications & Device Tokens
    /// </summary>
    [Route("api/v1/notifications")]
    [ApiController]
    [Produces("application/json")]
    [SwaggerTag("Notifications — الإشعارات")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator) => _mediator = mediator;

        private int GetDonorId() => int.TryParse(User.FindFirst("donorId")?.Value, out var id) ? id : 0;

        /// <summary>تسجيل رمز جهاز جديد لتلقي الإشعارات</summary>
        [HttpPost("tokens")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] تسجيل رمز الجهاز (FCM Token)",
            Description = "يستخدم لتسجيل الـ FCM Token الخاص بالجهاز لتلقي الإشعارات.",
            OperationId = "RegisterDeviceToken")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterDeviceTokenDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0) return Unauthorized(Result<bool>.Failure("Donor not found.", ErrorType.Unauthorized));
            
            return Ok(await _mediator.Send(new RegisterDeviceTokenCommand(donorId, dto.Token, dto.DeviceType), ct));
        }

        /// <summary>جلب تاريخ الإشعارات للمتبرع</summary>
        [HttpGet("history")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] تاريخ الإشعارات",
            Description = "يجلب قائمة بالإشعارات السابقة المرسلة للمتبرع.",
            OperationId = "GetNotificationHistory")]
        public async Task<IActionResult> GetHistory(CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0) return Unauthorized(Result<List<NotificationDto>>.Failure("Donor not found.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new GetNotificationHistoryQuery(donorId), ct));
        }

        /// <summary>تحديد إشعار كمقروء</summary>
        [HttpPatch("{id}/read")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] تحديد كمقروء",
            Description = "تغيير حالة الإشعار إلى مقروء.",
            OperationId = "MarkAsRead")]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0) return Unauthorized(Result<bool>.Failure("Donor not found.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new MarkNotificationAsReadCommand(id, donorId), ct));
        }

        /// <summary>إرسال إشعار عام لجميع المستخدمين (Admin Only)</summary>
        [HttpPost("broadcast")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] إرسال إشعار جماعي",
            Description = "إرسال إشعار لجميع الأجهزة المسجلة في النظام.",
            OperationId = "BroadcastNotification")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastNotificationDto dto, CancellationToken ct)
        {
            return Ok(await _mediator.Send(new SendBroadcastNotificationCommand(dto.Title, dto.Message, dto.Type, dto.RelatedEntityId), ct));
        }
    }
}
