using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Features.Subscriptions.Commands.CancelSubscription;
using BackEnd.Application.Features.Subscriptions.Commands.CreateAppointmentSlot;
using BackEnd.Application.Features.Subscriptions.Commands.CreateSubscription;
using BackEnd.Application.Features.Subscriptions.Commands.RejectPayment;
using BackEnd.Application.Features.Subscriptions.Commands.SubmitPayment;
using BackEnd.Application.Features.Subscriptions.Commands.VerifyPayment;
using BackEnd.Application.Features.Subscriptions.Queries.GetAvailableSlots;
using BackEnd.Application.Features.Subscriptions.Queries.GetDeliveryAreas;
using BackEnd.Application.Features.Subscriptions.Queries.GetMySubscriptions;
using BackEnd.Application.Features.Subscriptions.Queries.GetPendingPayments;
using BackEnd.Application.Features.Subscriptions.Commands.CreateDeliveryArea;
using BackEnd.Application.Features.Subscriptions.Commands.UpdateDeliveryArea;
using BackEnd.Application.Features.Subscriptions.Commands.DeleteDeliveryArea;
using BackEnd.Application.Features.Subscriptions.Queries.GetAdminDeliveryAreas;
using BackEnd.Application.Features.Subscriptions.Queries.GetPaymentDetails;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackEnd.Api.Controllers
{
    /// <summary>
    /// إدارة اشتراكات الكفالات — Subscription Management
    /// </summary>
    /// <remarks>
    /// يتضمن هذا Controller:
    /// - إنشاء اشتراك وإلغاؤه (Donor)
    /// - تقديم طلبات الدفع بكل الطرق المتاحة (Donor)
    /// - مراجعة وتأكيد/رفض الدفعات (Reception/Admin)
    /// - إدارة مناطق التوصيل ومواعيد الفروع (Admin)
    /// </remarks>
    [Route("api/v1/subscriptions")]
    [ApiController]
    [Produces("application/json")]
    [SwaggerTag("Subscriptions — اشتراكات الكفالات")]
    public class SubscriptionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubscriptionController(IMediator mediator) => _mediator = mediator;

        /// <summary>يستخرج donorId من JWT Claims</summary>
        private int GetDonorId() =>
            int.TryParse(User.FindFirst("donorId")?.Value, out var id) ? id : 0;

        /// <summary>يستخرج staffId من JWT Claims</summary>
        private int GetStaffId() =>
            int.TryParse(User.FindFirst("staffId")?.Value, out var id) ? id : 0;

        // =====================================================
        // DONOR ENDPOINTS
        // =====================================================

        /// <summary>إنشاء اشتراك كفالة جديد</summary>
        [HttpPost]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] إنشاء اشتراك كفالة",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> Create(
            [FromBody] CreateSubscriptionDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new CreateSubscriptionCommand(donorId, dto), ct));
        }

        /// <summary>تقديم طلب دفع لاشتراك (Deprecated)</summary>
        [HttpPost("{id:int}/submit-payment")]
        [Authorize(Roles = "Donor")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "[Donor] تقديم طلب دفع (قديم)",
            Description = "استخدم النقاط المتخصصة الجديدة /pay/electronic أو /pay/branch أو /pay/representative",
            Tags = new[] { "Subscriptions — Donor" })]
        [Obsolete("استخدم النقاط المتخصصة الجديدة")]
        public async Task<IActionResult> SubmitPayment(
            int id,
            [FromForm] SubmitPaymentDto dto,
            CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new SubmitPaymentCommand(id, donorId, dto), ct));
        }

        /// <summary>تقديم طلب دفع إلكتروني (فودافون كاش / إنستا باي)</summary>
        [HttpPost("{id:int}/pay/electronic")]
        [Authorize(Roles = "Donor")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "[Donor] دفع إلكتروني لاشتراك",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> PayElectronic(
            int id, [FromForm] BackEnd.Application.Dtos.EmergencyCase.ElectronicEmergencyDonationDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            return Ok(await _mediator.Send(new SubmitElectronicSubscriptionPaymentCommand(
                id, donorId, dto.Amount, (PaymentMethod)dto.PaymentMethod, dto.SenderPhoneNumber, dto.ReceiptImage), ct));
        }

        /// <summary>تقديم طلب دفع في الفرع</summary>
        [HttpPost("{id:int}/pay/branch")]
        [Authorize(Roles = "Donor")]
        [SwaggerOperation(
            Summary = "[Donor] دفع في الفرع لاشتراك",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> PayBranch(
            int id, [FromBody] BackEnd.Application.Dtos.EmergencyCase.BranchEmergencyDonationDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            return Ok(await _mediator.Send(new SubmitBranchSubscriptionPaymentCommand(
                id, donorId, dto.Amount, dto.SlotId, dto.BranchContactPhone, User.Identity?.Name ?? ""), ct));
        }

        /// <summary>تقديم طلب دفع عبر مندوب</summary>
        [HttpPost("{id:int}/pay/representative")]
        [Authorize(Roles = "Donor")]
        [SwaggerOperation(
            Summary = "[Donor] دفع عبر مندوب لاشتراك",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> PayRepresentative(
            int id, [FromBody] BackEnd.Application.Dtos.EmergencyCase.RepresentativeEmergencyDonationDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            return Ok(await _mediator.Send(new SubmitRepresentativeSubscriptionPaymentCommand(
                id, donorId, dto.Amount, dto.DeliveryAreaId, dto.Address, dto.ContactName, dto.ContactPhone, dto.RepresentativeNotes), ct));
        }

        /// <summary>جلب اشتراكات المتبرع الحالي</summary>
        [HttpGet("my")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SubscriptionDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] جلب اشتراكاتي",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> GetMine(CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new GetMySubscriptionsQuery(donorId), ct));
        }

        /// <summary>إلغاء اشتراك</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Donor")]
        [SwaggerOperation(
            Summary = "[Donor] إلغاء اشتراك",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> Cancel(
            int id,
            [FromBody] CancelSubscriptionRequest dto,
            CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new CancelSubscriptionCommand(id, donorId, dto.Reason), ct));
        }

        /// <summary>المواعيد المتاحة للحضور في الفرع</summary>
        [HttpGet("available-slots")]
        [Authorize(Roles = "Donor")]
        [SwaggerOperation(
            Summary = "[Donor] المواعيد المتاحة في المقر",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> GetAvailableSlots(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAvailableSlotsQuery(), ct));

        /// <summary>مناطق توصيل المناديب المتاحة</summary>
        [HttpGet("delivery-areas")]
        [Authorize(Roles = "Donor")]
        [SwaggerOperation(
            Summary = "[Donor] مناطق التوصيل المتاحة",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> GetDeliveryAreas(CancellationToken ct)
            => Ok(await _mediator.Send(new GetDeliveryAreasQuery(), ct));

        // =====================================================
        // STAFF ENDPOINTS (Reception + Admin)
        // =====================================================

        /// <summary>جلب طلبات الدفع المعلقة (كل الطرق)</summary>
        [HttpGet("payments/pending")]
        [Authorize(Roles = "Reception,Admin")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PaymentRequestSummaryDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Reception/Admin] كل طلبات الدفع المعلقة",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> GetPendingPayments(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingPaymentsQuery(), ct));

        /// <summary>جلب طلبات الدفع عبر المناديب المعلقة</summary>
        [HttpGet("payments/pending/representatives")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(
            Summary = "[Reception/Admin] طلبات المناديب المعلقة",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> GetPendingRepresentatives(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingPaymentsByMethodQuery(PaymentMethod.Representative), ct));

        /// <summary>جلب حجوزات الفرع المعلقة</summary>
        [HttpGet("payments/pending/branch")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(
            Summary = "[Reception/Admin] حجوزات الفرع المعلقة",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> GetPendingBranch(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingPaymentsByMethodQuery(PaymentMethod.Branch), ct));

        /// <summary>جلب طلبات Vodafone Cash المعلقة</summary>
        [HttpGet("payments/pending/vodafonecash")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(
            Summary = "[Reception/Admin] طلبات Vodafone Cash المعلقة",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> GetPendingVodafoneCash(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingPaymentsByMethodQuery(PaymentMethod.VodafoneCash), ct));

        /// <summary>جلب طلبات InstaPay المعلقة</summary>
        [HttpGet("payments/pending/instapay")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(
            Summary = "[Reception/Admin] طلبات InstaPay المعلقة",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> GetPendingInstaPay(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingPaymentsByMethodQuery(PaymentMethod.InstaPay), ct));

        /// <summary>جلب تفاصيل طلب دفع محدد</summary>
        [HttpGet("payments/{paymentId:int}")]
        [Authorize(Roles = "Reception,Admin")]
        [ProducesResponseType(typeof(ApiResponse<PaymentRequestDetailDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Reception/Admin] تفاصيل طلب دفع",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> GetPaymentDetails(int paymentId, CancellationToken ct)
            => Ok(await _mediator.Send(new GetPaymentDetailsQuery(paymentId), ct));

        /// <summary>تأكيد دفعة</summary>
        [HttpPost("payments/{paymentId:int}/verify")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(
            Summary = "[Reception/Admin] تأكيد دفعة",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> VerifyPayment(int paymentId, CancellationToken ct)
        {
            var staffId = GetStaffId();
            if (staffId == 0)
                return Ok(Result<object>.Failure("لم يتم التعرف على هوية الموظف.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new VerifyPaymentCommand(paymentId, staffId), ct));
        }

        /// <summary>رفض دفعة مع ذكر السبب</summary>
        [HttpPost("payments/{paymentId:int}/reject")]
        [Authorize(Roles = "Reception,Admin")]
        [SwaggerOperation(
            Summary = "[Reception/Admin] رفض دفعة",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> RejectPayment(int paymentId, [FromBody] RejectPaymentRequest dto, CancellationToken ct)
        {
            var staffId = GetStaffId();
            if (staffId == 0)
                return Ok(Result<object>.Failure("لم يتم التعرف على هوية الموظف.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new RejectPaymentCommand(paymentId, staffId, dto.Reason), ct));
        }

        // =====================================================
        // ADMIN ENDPOINTS
        // =====================================================

        /// <summary>إضافة منطقة توصيل جديدة</summary>
        [HttpPost("delivery-areas")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<CreateDeliveryAreaResponse>), StatusCodes.Status201Created)]
        [SwaggerOperation(
            Summary = "[Admin] إضافة منطقة توصيل",
            Tags = new[] { "Subscriptions — Admin" })]
        public async Task<IActionResult> AddDeliveryArea([FromBody] CreateDeliveryAreaCommand cmd, CancellationToken ct)
        {
            var result = await _mediator.Send(cmd, ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>تعديل منطقة توصيل</summary>
        [HttpPut("delivery-areas/{id:int}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "[Admin] تعديل منطقة توصيل",
            Tags = new[] { "Subscriptions — Admin" })]
        public async Task<IActionResult> UpdateDeliveryArea(int id, [FromBody] UpdateDeliveryAreaCommand cmd, CancellationToken ct)
        {
            if (id != cmd.Id) return BadRequest();
            return Ok(await _mediator.Send(cmd, ct));
        }

        /// <summary>حذف منطقة توصيل نهائياً</summary>
        [HttpDelete("delivery-areas/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<DeleteDeliveryAreaResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] حذف منطقة توصيل",
            Tags = new[] { "Subscriptions — Admin" })]
        public async Task<IActionResult> DeleteDeliveryArea(int id, CancellationToken ct)
            => Ok(await _mediator.Send(new DeleteDeliveryAreaCommand(id), ct));

        /// <summary>جلب كل المناطق للأدمن</summary>
        [HttpGet("delivery-areas/admin")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "[Admin] جلب كل المناطق",
            Tags = new[] { "Subscriptions — Admin" })]
        public async Task<IActionResult> GetAdminDeliveryAreas(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAdminDeliveryAreasQuery(), ct));

        /// <summary>إضافة موعد جديد في الفرع</summary>
        [HttpPost("slots")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "[Admin] إضافة موعد متاح في الفرع",
            Tags = new[] { "Subscriptions — Admin" })]
        public async Task<IActionResult> AddSlot([FromBody] CreateSlotRequest dto, CancellationToken ct)
            => Ok(await _mediator.Send(new CreateAppointmentSlotCommand(dto), ct));

        /// <summary>جلب كل المواعيد (Admin)</summary>
        [HttpGet("slots/admin")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "[Admin] جلب كل المواعيد",
            Tags = new[] { "Subscriptions — Admin" })]
        public async Task<IActionResult> GetAllSlots(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAvailableSlotsQuery(), ct));
    }
    }

    // =====================================================
    // Request Records
    // =====================================================

    /// <summary>طلب إلغاء الاشتراك</summary>
    public record CancelSubscriptionRequest(
        /// <summary>سبب الإلغاء (اختياري)</summary>
        string? Reason
    );

    /// <summary>طلب رفض الدفعة</summary>
    public record RejectPaymentRequest(
        /// <summary>سبب الرفض — يظهر للمتبرع (مطلوب)</summary>
        string Reason
    );
