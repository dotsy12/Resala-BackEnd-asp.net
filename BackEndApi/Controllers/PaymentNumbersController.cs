using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.PaymentNumber;
using BackEnd.Application.Features.Admin.PaymentNumbers.Commands.UpsertPaymentNumber;
using BackEnd.Application.Features.Admin.PaymentNumbers.Queries.GetAllPaymentNumbers;
using BackEnd.Application.Features.PaymentNumbers.Queries.GetActivePaymentNumbers;
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
    [Produces("application/json")]
    [SwaggerTag("Payment Numbers — إدارة أرقام تواصل الدفع (Vodafone Cash, InstaPay)")]
    public class PaymentNumbersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentNumbersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ═══════════════════════════════════════════════════
        //  1. PUBLIC — جلب الأرقام النشطة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// جلب أرقام تواصل الدفع النشطة (للمتبرعين)
        /// </summary>
        [HttpGet("api/v1/payment-numbers")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Result<IReadOnlyList<PaymentNumberDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "جلب أرقام الدفع النشطة",
            Description = "يرجع أرقام Vodafone Cash وحسابات InstaPay النشطة ليتم عرضها للمستخدم.",
            OperationId = "PaymentNumbers_GetActive",
            Tags = new[] { "Payment Numbers — Public" }
        )]
        public async Task<ActionResult<Result<IReadOnlyList<PaymentNumberDto>>>> GetActive(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActivePaymentNumbersQuery(), ct);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════
        //  2. ADMIN — إدارة الأرقام
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Admin] جلب كل أرقام الدفع
        /// </summary>
        [HttpGet("api/v1/admin/payment-numbers")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<IReadOnlyList<PaymentNumberDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] جلب كل أرقام الدفع",
            Description = "يرجع كل الأرقام المسجلة (نشطة وغير نشطة) للإدارة.",
            OperationId = "PaymentNumbers_GetAllAdmin",
            Tags = new[] { "Payment Numbers — Admin" }
        )]
        public async Task<ActionResult<Result<IReadOnlyList<PaymentNumberDto>>>> GetAllAdmin(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllPaymentNumbersAdminQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] إضافة أو تحديث رقم دفع (Upsert)
        /// </summary>
        /// <remarks>
        /// إذا كان النوع (VodafoneCash أو InstaPay) موجوداً مسبقاً، سيتم تحديثه.
        /// إذا لم يكن موجوداً، سيتم إنشاؤه.
        /// </remarks>
        [HttpPost("api/v1/admin/payment-numbers")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<PaymentNumberDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] إضافة أو تحديث رقم دفع",
            Description = "يقوم بعملية Upsert (إضافة أو تحديث) لرقم دفع بناءً على النوع.",
            OperationId = "PaymentNumbers_Upsert",
            Tags = new[] { "Payment Numbers — Admin" }
        )]
        public async Task<ActionResult<Result<PaymentNumberDto>>> Upsert([FromBody] UpsertPaymentNumberDto dto, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpsertPaymentNumberCommand(dto), ct);
            return Ok(result);
        }
    }
}
