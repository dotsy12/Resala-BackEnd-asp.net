using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SupportChat;
using BackEnd.Application.Features.SupportChat.Commands.MarkMessagesAsRead;
using BackEnd.Application.Features.SupportChat.Queries.GetChatHistory;
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
    [Route("api/v1/support")]
    [Produces("application/json")]
    [SwaggerTag("SupportChat — نظام الدعم الفني والمحادثة الفورية")]
    [Authorize]
    public class SupportChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SupportChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// جلب سجل محادثة الدعم الفني
        /// </summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(Result<PagedResult<SupportMessageDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "جلب سجل محادثة الدعم الفني",
            Description = "يرجع قائمة مرقمة بالرسائل السابقة لغرفة المحادثة الخاصة بالمستخدم المحدد، مرتبة تنازلياً حسب تاريخ الإنشاء.",
            OperationId = "SupportChat_GetHistory",
            Tags = new[] { "SupportChat" }
        )]
        public async Task<ActionResult<Result<PagedResult<SupportMessageDto>>>> GetHistory(
            [FromQuery] string chatOwnerUserId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await _mediator.Send(new GetChatHistoryQuery(chatOwnerUserId, pageNumber, pageSize), ct);
            return Ok(result);
        }

        /// <summary>
        /// تحديد رسائل الدعم الفني كمقروءة
        /// </summary>
        [HttpPost("read/{userId}")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "تحديد الرسائل كمقروءة",
            Description = "يحدث حالة جميع الرسائل غير المقروءة في غرفة المحادثة المحددة لتصبح مقروءة.",
            OperationId = "SupportChat_MarkAsRead",
            Tags = new[] { "SupportChat" }
        )]
        public async Task<ActionResult<Result<bool>>> MarkAsRead(string userId, CancellationToken ct)
        {
            var result = await _mediator.Send(new MarkMessagesAsReadCommand(userId), ct);
            return Ok(result);
        }
    }
}
