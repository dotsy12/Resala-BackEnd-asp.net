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
    /// - إدارة مواعيد الفروع (Admin)
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
        /// <remarks>
        /// يُنشئ اشتراكاً للمتبرع في برنامج كفالة محدد.
        ///
        /// **القواعد:**
        /// - لا يمكن الاشتراك مرتين في نفس الكفالة
        /// - الكفالة يجب أن تكون نشطة
        /// - المبلغ يجب أن يكون أكبر من صفر
        ///
        /// **دورات الدفع المتاحة:**
        /// - `1` = شهري
        /// - `3` = ربع سنوي
        /// - `6` = نصف سنوي
        ///
        /// **مثال:**
        /// ```json
        /// {
        ///   "sponsorshipId": 1,
        ///   "amount": 500,
        ///   "paymentCycle": 1
        /// }
        /// ```
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [SwaggerOperation(
            Summary = "[Donor] إنشاء اشتراك كفالة",
            Description = "يُنشئ اشتراكاً جديداً للمتبرع في برنامج كفالة — يتطلب دور Donor",
            OperationId = "Subscriptions_Create",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> Create(
            [FromBody] CreateSubscriptionDto dto, CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure(
                    "لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new CreateSubscriptionCommand(donorId, dto), ct));
        }

        /// <summary>تقديم طلب دفع لاشتراك</summary>
        /// <remarks>
        /// يُقدِّم المتبرع طلب دفع لاشتراكه باستخدام إحدى طرق الدفع المتاحة.
        ///
        /// **طرق الدفع:**
        ///
        /// 1. **فودافون كاش (Method=1)**
        ///    - `SenderPhoneNumber`: رقم الهاتف الذي تم التحويل منه
        ///    - `ReceiptImage`: صورة الإيصال (jpg/png/pdf — max 5MB)
        ///
        /// 2. **InstaPay (Method=2)**
        ///    - `SenderPhoneNumber`: رقم الهاتف
        ///    - `ReceiptImage`: صورة الإيصال
        ///
        /// 3. **الحضور للفرع (Method=3)**
        ///    - `SlotId`: معرف الموعد المتاح (من GET /available-slots)
        ///    - `DonorName`: الاسم الذي سيظهر للموظف
        ///    - `BranchContactPhone`: رقم هاتف للتواصل
        ///
        /// 4. **مندوب (Method=4)**
        ///    - `DeliveryAreaId`: معرف منطقة التوصيل (من GET /delivery-areas)
        ///    - `ContactName`: الاسم
        ///    - `ContactPhone`: رقم الهاتف
        ///    - `Address`: العنوان الكامل
        ///    - `RepresentativeNotes`: ملاحظات إضافية (اختياري)
        ///
        /// **ملاحظة:** يجب إرسال الطلب كـ `multipart/form-data`
        /// </remarks>
        [HttpPost("{id:int}/submit-payment")]
        [Authorize(Roles = "Donor")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB max request
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Donor] تقديم طلب دفع",
            Description = "يُقدِّم المتبرع طلب دفع لاشتراكه. يدعم VodafoneCash, InstaPay, Branch, Representative.",
            OperationId = "Subscriptions_SubmitPayment",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> SubmitPayment(
            /// <param name="id">معرف الاشتراك</param>
            int id,
            [FromForm] SubmitPaymentDto dto,
            CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure(
                    "لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new SubmitPaymentCommand(id, donorId, dto), ct));
        }

        /// <summary>جلب اشتراكات المتبرع الحالي</summary>
        /// <remarks>
        /// يرجع قائمة بجميع اشتراكات المتبرع المُسجَّل حالياً، مع آخر طلبات الدفع لكل اشتراك.
        ///
        /// **يتضمن الاشتراكات:**
        /// - النشطة (Active)
        /// - الملغاة (Cancelled)
        /// - المعلَّقة بسبب التأخر (Suspended)
        /// </remarks>
        [HttpGet("my")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SubscriptionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "[Donor] جلب اشتراكاتي",
            Description = "يرجع قائمة بجميع اشتراكات المتبرع الحالي مع آخر طلبات الدفع.",
            OperationId = "Subscriptions_GetMine",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> GetMine(CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure(
                    "لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new GetMySubscriptionsQuery(donorId), ct));
        }

        /// <summary>إلغاء اشتراك</summary>
        /// <remarks>
        /// يُلغي المتبرع اشتراكه في كفالة معينة.
        ///
        /// **القواعد:**
        /// - فقط المتبرع صاحب الاشتراك يمكنه الإلغاء
        /// - الاشتراك يجب أن يكون في حالة Active أو Suspended
        /// - يمكن إضافة سبب الإلغاء (اختياري)
        ///
        /// **مثال:**
        /// ```json
        /// { "reason": "ظروف مادية" }
        /// ```
        /// </remarks>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Donor] إلغاء اشتراك",
            Description = "يُلغي المتبرع اشتراكه — يتطلب أن يكون الاشتراك نشطاً أو معلَّقاً، وأن يكون المتبرع هو الصاحب.",
            OperationId = "Subscriptions_Cancel",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> Cancel(
            /// <param name="id">معرف الاشتراك</param>
            int id,
            [FromBody] CancelSubscriptionRequest dto,
            CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure(
                    "لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(
                new CancelSubscriptionCommand(id, donorId, dto.Reason), ct));
        }

        /// <summary>المواعيد المتاحة للحضور في الفرع</summary>
        /// <remarks>
        /// يرجع قائمة بالمواعيد المتاحة في الفرع لاختيار موعد الدفع.
        ///
        /// **الاستخدام:** عند اختيار طريقة الدفع `Branch (Method=3)`,
        /// استخدم الـ `SlotId` من هذه القائمة في `SubmitPayment`.
        ///
        /// **يُرجع فقط:** المواعيد المستقبلية التي لم تمتلئ بعد.
        /// </remarks>
        [HttpGet("available-slots")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentSlotDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "[Donor] المواعيد المتاحة في المقر",
            Description = "يرجع قائمة بالمواعيد المستقبلية المتاحة للحضور في الفرع. استخدم SlotId في submit-payment.",
            OperationId = "Subscriptions_GetAvailableSlots",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> GetAvailableSlots(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAvailableSlotsQuery(), ct));

        /// <summary>مناطق توصيل المناديب المتاحة</summary>
        /// <remarks>
        /// يرجع قائمة بمناطق التوصيل المتاحة لاختيار منطقة عند إرسال مندوب.
        ///
        /// **الاستخدام:** عند اختيار طريقة الدفع `Representative (Method=4)`,
        /// استخدم الـ `DeliveryAreaId` من هذه القائمة في `SubmitPayment`.
        /// </remarks>
        [HttpGet("delivery-areas")]
        [Authorize(Roles = "Donor")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DeliveryAreaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "[Donor] مناطق التوصيل المتاحة",
            Description = "يرجع قائمة مناطق توصيل المناديب النشطة. استخدم DeliveryAreaId في submit-payment.",
            OperationId = "Subscriptions_GetDeliveryAreas",
            Tags = new[] { "Subscriptions — Donor" })]
        public async Task<IActionResult> GetDeliveryAreas(CancellationToken ct)
            => Ok(await _mediator.Send(new GetDeliveryAreasQuery(), ct));

        // =====================================================
        // STAFF ENDPOINTS (Reception + Admin)
        // =====================================================

        /// <summary>جلب طلبات الدفع المعلقة (كل الطرق)</summary>
        /// <remarks>
        /// يرجع جميع طلبات الدفع التي بحالة `Pending` بغض النظر عن طريقة الدفع.
        ///
        /// **مفيد لـ:** Dashboard الموظف لعرض كل الطلبات المنتظِرة.
        ///
        /// **الطلبات تشمل:** VodafoneCash, InstaPay, Branch, Representative
        /// </remarks>
        [HttpGet("payments/pending")]
        [Authorize(Roles = "Reception,Admin")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<object>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Reception/Admin] طلبات الدفع المعلقة",
            Description = "يرجع كل طلبات الدفع بحالة Pending من كل طرق الدفع. للموظفين فقط.",
            OperationId = "Subscriptions_GetPendingPayments",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> GetPendingPayments(CancellationToken ct)
            => Ok(await _mediator.Send(new GetPendingPaymentsQuery(), ct));

        /// <summary>جلب طلبات الدفع عبر المناديب</summary>
        /// <remarks>
        /// يرجع طلبات الدفع المعلقة التي اختار أصحابها طريقة المندوب فقط.
        ///
        /// **الاستخدام:** لتوزيع المناديب وتعيينهم للطلبات المنتظِرة.
        /// </remarks>
        [HttpGet("payments/pending/representatives")]
        [Authorize(Roles = "Reception,Admin")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<object>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Reception/Admin] طلبات المناديب المعلقة",
            Description = "يرجع طلبات الدفع بطريقة المندوب والحالة Pending. لتوزيع المناديب.",
            OperationId = "Subscriptions_GetPendingRepresentatives",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> GetPendingRepresentatives(CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetPendingPaymentsByMethodQuery(PaymentMethod.Representative), ct));

        /// <summary>جلب حجوزات الفرع المعلقة</summary>
        /// <remarks>
        /// يرجع طلبات الدفع المعلقة التي اختار أصحابها الحضور للفرع.
        ///
        /// **الاستخدام:** لتجهيز استقبال المتبرعين في المواعيد المحددة.
        /// </remarks>
        [HttpGet("payments/pending/branch")]
        [Authorize(Roles = "Reception,Admin")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<object>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Reception/Admin] حجوزات الفرع المعلقة",
            Description = "يرجع طلبات الدفع بطريقة الفرع والحالة Pending. لتجهيز استقبال المتبرعين.",
            OperationId = "Subscriptions_GetPendingBranch",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> GetPendingBranch(CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetPendingPaymentsByMethodQuery(PaymentMethod.Branch), ct));

        /// <summary>تأكيد دفعة</summary>
        /// <remarks>
        /// يُؤكِّد الموظف استلام الدفعة وتحديث حالة الاشتراك.
        ///
        /// **ما يحدث عند التأكيد:**
        /// 1. حالة طلب الدفع → `Verified`
        /// 2. `NextPaymentDate` للاشتراك تتقدم بمقدار دورة الدفع
        /// 3. إذا كان الاشتراك `Suspended` → يعود `Active`
        /// 4. يُرسَل إشعار للمتبرع
        ///
        /// **ملاحظة:** لا يمكن التأكيد على دفعة محذوفة أو مرفوضة مسبقاً.
        /// </remarks>
        [HttpPost("payments/{paymentId:int}/verify")]
        [Authorize(Roles = "Reception,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Reception/Admin] تأكيد دفعة",
            Description = "يُؤكِّد الموظف استلام الدفعة. يُحدِّث NextPaymentDate ويُرسل إشعاراً للمتبرع.",
            OperationId = "Subscriptions_VerifyPayment",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> VerifyPayment(
            /// <param name="paymentId">معرف طلب الدفع</param>
            int paymentId,
            CancellationToken ct)
        {
            var staffId = GetStaffId();
            if (staffId == 0)
                return Ok(Result<object>.Failure(
                    "لم يتم التعرف على هوية الموظف.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(new VerifyPaymentCommand(paymentId, staffId), ct));
        }

        /// <summary>رفض دفعة مع ذكر السبب</summary>
        /// <remarks>
        /// يرفض الموظف طلب الدفع مع إرسال سبب الرفض للمتبرع.
        ///
        /// **ما يحدث عند الرفض:**
        /// 1. حالة طلب الدفع → `Rejected`
        /// 2. سبب الرفض يُحفَظ ويظهر للمتبرع
        /// 3. يُرسَل إشعار للمتبرع بسبب الرفض
        ///
        /// **أمثلة على أسباب الرفض:**
        /// - "الصورة غير واضحة — يرجى إعادة الإرسال"
        /// - "المبلغ في الإيصال لا يطابق مبلغ الاشتراك"
        /// - "رقم الهاتف المُرسِل غير مطابق"
        ///
        /// **مثال:**
        /// ```json
        /// { "reason": "الصورة غير واضحة — يرجى إعادة الإرسال" }
        /// ```
        /// </remarks>
        [HttpPost("payments/{paymentId:int}/reject")]
        [Authorize(Roles = "Reception,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Reception/Admin] رفض دفعة",
            Description = "يرفض الموظف طلب الدفع مع إرسال سبب الرفض. يُرسل إشعاراً للمتبرع بالسبب.",
            OperationId = "Subscriptions_RejectPayment",
            Tags = new[] { "Subscriptions — Staff" })]
        public async Task<IActionResult> RejectPayment(
            /// <param name="paymentId">معرف طلب الدفع</param>
            int paymentId,
            [FromBody] RejectPaymentRequest dto,
            CancellationToken ct)
        {
            var staffId = GetStaffId();
            if (staffId == 0)
                return Ok(Result<object>.Failure(
                    "لم يتم التعرف على هوية الموظف.", ErrorType.Unauthorized));

            return Ok(await _mediator.Send(
                new RejectPaymentCommand(paymentId, staffId, dto.Reason), ct));
        }

        // =====================================================
        // ADMIN ENDPOINTS
        // =====================================================

        /// <summary>إضافة موعد جديد في الفرع</summary>
        /// <remarks>
        /// يُضيف Admin موعداً متاحاً للحضور في الفرع لاستقبال المتبرعين.
        ///
        /// **الحقول:**
        /// - `slotDate`: تاريخ الموعد (يجب أن يكون مستقبلياً)
        /// - `openFrom`: وقت الفتح (مثال: "09:00:00")
        /// - `openTo`: وقت الإغلاق (مثال: "14:00:00")
        /// - `maxCapacity`: أقصى عدد للحجوزات
        /// - `notes`: ملاحظات اختيارية
        ///
        /// **مثال:**
        /// ```json
        /// {
        ///   "slotDate": "2026-05-15",
        ///   "openFrom": "09:00:00",
        ///   "openTo": "14:00:00",
        ///   "maxCapacity": 20,
        ///   "notes": "متاح أيام الأسبوع فقط"
        /// }
        /// ```
        /// </remarks>
        [HttpPost("slots")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<AppointmentSlotDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Admin] إضافة موعد متاح في الفرع",
            Description = "يُنشئ Admin موعداً متاحاً لاستقبال المتبرعين في الفرع. يتطلب دور Admin.",
            OperationId = "Subscriptions_AddSlot",
            Tags = new[] { "Subscriptions — Admin" })]
        public async Task<IActionResult> AddSlot(
            [FromBody] CreateSlotRequest dto, CancellationToken ct)
            => Ok(await _mediator.Send(new CreateAppointmentSlotCommand(dto), ct));

        /// <summary>جلب كل المواعيد (Admin)</summary>
        /// <remarks>
        /// يرجع جميع المواعيد بما فيها المنتهية والممتلئة.
        ///
        /// **الفرق عن GET /available-slots:**
        /// - `available-slots`: للمتبرع — يرجع فقط المستقبلية + غير الممتلئة
        /// - `slots` (هذا الـ endpoint): للـ Admin — يرجع كل المواعيد
        /// </remarks>
        [HttpGet("slots")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentSlotDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Admin] جلب كل المواعيد",
            Description = "يرجع جميع مواعيد الفرع بما فيها المنتهية. للـ Admin فقط.",
            OperationId = "Subscriptions_GetAllSlots",
            Tags = new[] { "Subscriptions — Admin" })]
        public async Task<IActionResult> GetAllSlots(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAvailableSlotsQuery(), ct));
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
}