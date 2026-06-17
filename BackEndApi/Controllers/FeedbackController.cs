using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Feedback;
using BackEnd.Application.Features.Feedbacks.Commands.SubmitFeedback;
using BackEnd.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus;
using BackEnd.Application.Features.Feedbacks.Queries.GetAllFeedbacks;
using BackEnd.Application.Features.Feedbacks.Queries.GetFeedbackDetails;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace BackEndApi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [SwaggerTag("Feedback — نظام التواصل والشكاوى")]
    public class FeedbackController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FeedbackController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ═══════════════════════════════════════════════════
        //  1. DONOR — إرسال الملاحظات والشكاوى
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Donor] إرسال ملاحظة أو شكوى
        /// </summary>
        [HttpPost("api/v1/donor/feedback")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(Result<FeedbackDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] إرسال ملاحظة أو شكوى",
            Description = "يسمح للمتبرع المسجل بإرسال ملاحظاته أو شكواه للمؤسسة.",
            OperationId = "Feedback_Submit",
            Tags = new[] { "Feedback — Donor" }
        )]
        public async Task<ActionResult<Result<FeedbackDto>>> Submit([FromBody] SubmitFeedbackCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════
        //  2. ADMIN — إدارة الملاحظات والشكاوى
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Admin] جلب كل الملاحظات والشكاوى
        /// </summary>
        [HttpGet("api/v1/admin/feedback")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<PagedResult<FeedbackDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] جلب كل الملاحظات والشكاوى",
            Description = "يرجع قائمة مرقمة بكل الرسائل الواردة مع إمكانية الفلترة حسب النوع أو الحالة.",
            OperationId = "Feedback_GetAll",
            Tags = new[] { "Feedback — Admin" }
        )]
        public async Task<ActionResult<Result<PagedResult<FeedbackDto>>>> GetAllAdmin(
            [FromQuery] FeedbackType? type, 
            [FromQuery] FeedbackStatus? status, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            CancellationToken ct = default)
        {
            var result = await _mediator.Send(new GetAllFeedbacksQuery(type, status, pageNumber, pageSize), ct);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] جلب تفاصيل ملاحظة/شكوى
        /// </summary>
        [HttpGet("api/v1/admin/feedback/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<FeedbackDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<FeedbackDto>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Admin] جلب تفاصيل الرسالة",
            Description = "يرجع تفاصيل كاملة عن الرسالة المحددة.",
            OperationId = "Feedback_GetDetails",
            Tags = new[] { "Feedback — Admin" }
        )]
        public async Task<ActionResult<Result<FeedbackDto>>> GetDetailsAdmin(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetFeedbackDetailsQuery(id), ct);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] تغيير حالة الملاحظة/الشكوى
        /// </summary>
        [HttpPatch("api/v1/admin/feedback/{id:int}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Admin] تغيير حالة الرسالة",
            Description = "تحديث حالة الرسالة إلى (Pending, Resolved, Rejected).",
            OperationId = "Feedback_UpdateStatus",
            Tags = new[] { "Feedback — Admin" }
        )]
        public async Task<ActionResult<Result<bool>>> UpdateStatusAdmin(int id, [FromBody] FeedbackStatus status, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateFeedbackStatusCommand(id, status), ct);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }
    }
}
