using BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Commands.DeleteEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Commands.UpdateEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Commands.DonateToEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Queries.GetAllEmergenciesCasies;
using BackEnd.Application.Features.EmergencyCase.Queries.GetEmergencyCaseById;
using BackEnd.Application.Features.EmergencyCase.Queries.GetPendingEmergencyCasePayments;
using BackEnd.Application.Features.Subscriptions.Commands.VerifyPayment;
using BackEnd.Application.Features.Subscriptions.Commands.RejectPayment;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Dtos.EmergencyCase;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackEnd.API.Controllers
{
    [ApiController]
    [Route("api/v1/emergency-cases")]
    [Produces("application/json")]
    [SwaggerTag("Emergency Cases — إدارة الحالات الحرجة والتبرعات")]
    public class EmergencyCasesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmergencyCasesController(IMediator mediator)
            => _mediator = mediator;

        /// <summary>يستخرج donorId من JWT Claims</summary>
        private int GetDonorId() =>
            int.TryParse(User.FindFirst("donorId")?.Value, out var id) ? id : 0;

        /// <summary>يستخرج staffId من JWT Claims</summary>
        private int GetStaffId() =>
            int.TryParse(User.FindFirst("staffId")?.Value, out var id) ? id : 0;

        // ═══════════════════════════════════════════════════
        //  1. DONOR FLOW — التبرع المباشر
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Donor] تبرع مباشر لحالة طوارئ (Deprecated)
        /// </summary>
        /// <remarks>
        /// يُنصح باستخدام النقاط المتخصصة الجديدة:
        /// - `/donate/electronic`
        /// - `/donate/representative`
        /// - `/donate/branch`
        /// </remarks>
        [HttpPost("{id:int}/donate")]
        [Authorize(Roles = "Donor")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDonationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "[Donor] تبرع مباشر لحالة طوارئ (قديم)",
            Tags = new[] { "EmergencyCases — Donor" }
        )]
        [Obsolete("استخدم النقاط المتخصصة الجديدة /donate/electronic أو /donate/representative أو /donate/branch")]
        public async Task<IActionResult> Donate(int id, [FromForm] SubmitPaymentDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0) return Unauthorized();

            return Ok(await _mediator.Send(new DonateToEmergencyCaseCommand(id, donorId, dto), ct));
        }

        /// <summary>
        /// [Donor] تبرع إلكتروني (فودافون كاش / إنستا باي)
        /// </summary>
        [HttpPost("{id:int}/donate/electronic")]
        [Authorize(Roles = "Donor")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDonationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "[Donor] تبرع إلكتروني لحالة طوارئ",
            Description = "يدعم VodafoneCash (1) و InstaPay (2). يتطلب رفع صورة الإيصال ورقم الهاتف.",
            Tags = new[] { "EmergencyCases — Donor" }
        )]
        public async Task<IActionResult> DonateElectronic(int id, [FromForm] ElectronicEmergencyDonationDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0) return Unauthorized();

            return Ok(await _mediator.Send(new DonateElectronicToEmergencyCaseCommand(id, donorId, dto), ct));
        }

        /// <summary>
        /// [Donor] تبرع عن طريق مندوب تحصيل
        /// </summary>
        [HttpPost("{id:int}/donate/representative")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDonationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "[Donor] تبرع عن طريق مندوب",
            Description = "طلب مندوب للتحصيل من العنوان المذكور.",
            Tags = new[] { "EmergencyCases — Donor" }
        )]
        public async Task<IActionResult> DonateRepresentative(int id, [FromBody] RepresentativeEmergencyDonationDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0) return Unauthorized();

            return Ok(await _mediator.Send(new DonateRepresentativeToEmergencyCaseCommand(id, donorId, dto), ct));
        }

        /// <summary>
        /// [Donor] تبرع في أحد فروع المؤسسة
        /// </summary>
        [HttpPost("{id:int}/donate/branch")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDonationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "[Donor] تبرع في الفرع",
            Description = "حجز موعد للتبرع في أحد الفروع.",
            Tags = new[] { "EmergencyCases — Donor" }
        )]
        public async Task<IActionResult> DonateBranch(int id, [FromBody] BranchEmergencyDonationDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0) return Unauthorized();

            return Ok(await _mediator.Send(new DonateBranchToEmergencyCaseCommand(id, donorId, dto), ct));
        }

        // ═══════════════════════════════════════════════════
        //  2. STAFF FLOW — مراجعة التبرعات
        // ═══════════════════════════════════════════════════

        /// <summary>جلب تبرعات حالات الطوارئ المعلقة</summary>
        [HttpGet("payments/pending")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(Summary = "[Reception/Admin] كل تبرعات الطوارئ المعلقة", Tags = new[] { "EmergencyCases — Staff" })]
        public async Task<IActionResult> GetPendingPayments(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingEmergencyCasePaymentsQuery(), ct));

        /// <summary>جلب تبرعات حالات الطوارئ المعلقة (VodafoneCash)</summary>
        [HttpGet("payments/pending/vodafonecash")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(Summary = "[Reception/Admin] تبرعات VodafoneCash المعلقة", Tags = new[] { "EmergencyCases — Staff" })]
        public async Task<IActionResult> GetPendingVodafoneCash(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingEmergencyCasePaymentsQuery(PaymentMethod.VodafoneCash), ct));

        /// <summary>جلب تبرعات حالات الطوارئ المعلقة (InstaPay)</summary>
        [HttpGet("payments/pending/instapay")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(Summary = "[Reception/Admin] تبرعات InstaPay المعلقة", Tags = new[] { "EmergencyCases — Staff" })]
        public async Task<IActionResult> GetPendingInstaPay(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingEmergencyCasePaymentsQuery(PaymentMethod.InstaPay), ct));

        /// <summary>جلب تبرعات حالات الطوارئ المعلقة (الفرع)</summary>
        [HttpGet("payments/pending/branch")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(Summary = "[Reception/Admin] تبرعات الفرع المعلقة", Tags = new[] { "EmergencyCases — Staff" })]
        public async Task<IActionResult> GetPendingBranch(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingEmergencyCasePaymentsQuery(PaymentMethod.Branch), ct));

        /// <summary>جلب تبرعات حالات الطوارئ المعلقة (المندوب)</summary>
        [HttpGet("payments/pending/representative")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(Summary = "[Reception/Admin] تبرعات المندوب المعلقة", Tags = new[] { "EmergencyCases — Staff" })]
        public async Task<IActionResult> GetPendingRepresentative(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingEmergencyCasePaymentsQuery(PaymentMethod.Representative), ct));

        /// <summary>تأكيد تبرع حالة طوارئ</summary>
        [HttpPost("payments/{paymentId:int}/verify")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(Summary = "[Reception/Admin] تأكيد تبرع", Tags = new[] { "EmergencyCases — Staff" })]
        public async Task<IActionResult> VerifyPayment(int paymentId, CancellationToken ct)
        {
            var staffId = GetStaffId();
            if (staffId == 0) return Unauthorized();
            return Ok(await _mediator.Send(new VerifyPaymentCommand(paymentId, staffId), ct));
        }

        /// <summary>رفض تبرع حالة طوارئ</summary>
        [HttpPost("payments/{paymentId:int}/reject")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(Summary = "[Reception/Admin] رفض تبرع", Tags = new[] { "EmergencyCases — Staff" })]
        public async Task<IActionResult> RejectPayment(int paymentId, [FromBody] RejectPaymentRequest dto, CancellationToken ct)
        {
            var staffId = GetStaffId();
            if (staffId == 0) return Unauthorized();
            return Ok(await _mediator.Send(new RejectPaymentCommand(paymentId, staffId, dto.Reason), ct));
        }

        // ═══════════════════════════════════════════════════
        //  3. ADMIN CRUD
        // ═══════════════════════════════════════════════════

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(Summary = "[Admin] إنشاء حالة حرجة", OperationId = "EmergencyCases_Create", Tags = new[] { "EmergencyCases — Admin" })]
        public async Task<IActionResult> Create([FromForm] CreateEmergencyCaseRequest request, CancellationToken ct)
        {
            var cmd = new CreateEmergencyCaseCommand(request.Title, request.Description, request.UrgencyLevel, request.RequiredAmount, request.Attachment);
            return Ok(await _mediator.Send(cmd, ct));
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "جلب كل الحالات الحرجة", OperationId = "EmergencyCases_GetAll", Tags = new[] { "EmergencyCases — Public" })]
        public async Task<ActionResult<IEnumerable<EmergencyCaseViewModel>>> GetAll(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAllEmergencyCasesQuery(), ct));

        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "جلب حالة حرجة بالـ ID", OperationId = "EmergencyCases_GetById", Tags = new[] { "EmergencyCases — Public" })]
        public async Task<ActionResult<EmergencyCaseViewModel>> GetById(int id, CancellationToken ct)
            => Ok(await _mediator.Send(new GetEmergencyCaseByIdQuery(id), ct));

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(Summary = "[Admin] تعديل حالة حرجة", OperationId = "EmergencyCases_Update", Tags = new[] { "EmergencyCases — Admin" })]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateEmergencyCaseRequest request, CancellationToken ct)
        {
            var cmd = new UpdateEmergencyCaseCommand(id, request.Title, request.Description, request.UrgencyLevel, request.RequiredAmount, request.Attachment, request.IsActive);
            return Ok(await _mediator.Send(cmd, ct));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "[Admin] حذف حالة حرجة", OperationId = "EmergencyCases_Delete", Tags = new[] { "EmergencyCases — Admin" })]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
            => Ok(await _mediator.Send(new DeleteEmergencyCaseCommand(id), ct));
    }

    public record CreateEmergencyCaseRequest(string Title, string Description, UrgencyLevel UrgencyLevel, decimal RequiredAmount, IFormFile? Attachment);
    public record UpdateEmergencyCaseRequest(string Title, string Description, UrgencyLevel UrgencyLevel, decimal? RequiredAmount, IFormFile? Attachment, bool IsActive);
}
