using BackEnd.Application.Features.Auth.Commands;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackEnd.Api.Controllers
{
    /// <summary>
    /// Authentication Module — إدارة المصادقة والتسجيل
    /// يغطي هذا الـ Controller كل عمليات المصادقة:
    /// تسجيل المتبرعين، الدخول، التحقق من الإيميل، واسترجاع الباسورد.
    /// </summary>
    [Route("api/v1/auth")]
    [ApiController]
    [Produces("application/json")]
    [SwaggerTag("Authentication — عمليات التسجيل والدخول واسترجاع الباسورد")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;

        // ═══════════════════════════════════════════════════════════════
        //  1. REGISTER — تسجيل متبرع جديد (عام)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// تسجيل متبرع جديد (Register Donor)
        /// </summary>
        /// <remarks>
        /// ينشئ حساباً جديداً للمتبرع ويُرسل OTP على بريده الإلكتروني للتحقق.
        /// الحساب يظل **غير مفعّل** حتى يتم التحقق عبر `/verify-email`.
        ///
        /// **ملاحظات:**
        /// - رقم الهاتف يُستخدم كـ username للدخول لاحقاً
        /// - الـ OTP صالح لمدة **10 دقائق** فقط
        /// - يُرسل بريد إلكتروني تلقائياً بعد التسجيل
        ///
        /// **Request Example:**
        /// ```json
        /// {
        ///   "name": "Ahmed Mohamed",
        ///   "email": "ahmed@example.com",
        ///   "phoneNumber": "01012345678",
        ///   "password": "SecurePass123",
        ///   "job": "Software Engineer",
        ///   "landline": "0223456789"
        /// }
        /// ```
        ///
        /// **Success Response (201):**
        /// ```json
        /// {
        ///   "succeeded": true,
        ///   "message": "Registration successful. Check your email for OTP.",
        ///   "data": { "userId": 42 }
        /// }
        /// ```
        /// </remarks>
        /// <param name="cmd">بيانات التسجيل — الاسم والإيميل والهاتف والباسورد</param>
        /// <response code="201">تم إنشاء الحساب بنجاح — يُرسل OTP على الإيميل</response>
        /// <response code="409">رقم الهاتف أو الإيميل مسجّل مسبقاً</response>
        /// <response code="400">بيانات غير صحيحة أو باسورد ضعيف</response>
        /// <response code="422">فشل في التحقق من صحة البيانات</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiSuccessResponse<RegisterDonorResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "تسجيل متبرع جديد",
            Description = "ينشئ حساباً جديداً ويُرسل OTP للتحقق من الإيميل",
            OperationId = "Auth_RegisterDonor",
            Tags = new[] { "Auth — Public" }
        )]
        public async Task<IActionResult> Register([FromBody] RegisterDonorCommand cmd)
            => Ok(await _mediator.Send(cmd));


        // ═══════════════════════════════════════════════════════════════
        //  2. VERIFY EMAIL — التحقق من الإيميل
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// التحقق من الإيميل بالـ OTP (Verify Email)
        /// </summary>
        /// <remarks>
        /// يتحقق من صحة الـ OTP المُرسل على الإيميل ويُفعّل الحساب.
        /// بعد هذه الخطوة يصبح المستخدم قادراً على تسجيل الدخول.
        ///
        /// **Request Example:**
        /// ```json
        /// {
        ///   "email": "ahmed@example.com",
        ///   "otp": "847291"
        /// }
        /// ```
        ///
        /// **Success Response (200):**
        /// ```json
        /// {
        ///   "succeeded": true,
        ///   "message": "Email verified successfully. You can now log in.",
        ///   "data": null
        /// }
        /// ```
        /// </remarks>
        /// <param name="cmd">الإيميل + رمز OTP المكوّن من 6 أرقام</param>
        /// <response code="200">تم التحقق وتفعيل الحساب بنجاح</response>
        /// <response code="400">الـ OTP منتهي الصلاحية أو غير صحيح</response>
        /// <response code="404">الإيميل غير مسجّل في النظام</response>
        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiSuccessResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "التحقق من الإيميل بالـ OTP",
            Description = "يُفعّل الحساب بعد التحقق من رمز OTP المُرسل بالبريد",
            OperationId = "Auth_VerifyEmail",
            Tags = new[] { "Auth — Public" }
        )]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand cmd)
            => Ok(await _mediator.Send(cmd));


        // ═══════════════════════════════════════════════════════════════
        //  3. LOGIN — تسجيل الدخول (للكل)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// تسجيل الدخول (Login)
        /// </summary>
        /// <remarks>
        /// يدعم **نوعين من الدخول:**
        ///
        /// **1. المتبرع (Donor)** — يدخل بـ **رقم الهاتف**:
        /// ```json
        /// {
        ///   "phoneNumber": "+201012345678",
        ///   "password": "SecurePass123"
        /// }
        /// ```
        ///
        /// **2. الموظف / الأدمن (Staff / Admin)** — يدخل بـ **Username**:
        /// ```json
        /// {
        ///   "username": "reception_ahmed",
        ///   "password": "StaffPass456"
        /// }
        /// ```
        ///
        /// **Success Response (200):**
        /// ```json
        /// {
        ///   "succeeded": true,
        ///   "message": "Login successful.",
        ///   "data": {
        ///     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///     "role": "Donor",
        ///     "userId": 42,
        ///     "name": "Ahmed Mohamed",
        ///     "phoneNumber": "01012345678"
        ///   }
        /// }
        /// ```
        ///
        /// **استخدام الـ Token:**
        /// أضف الـ Token في كل Request تالي في الـ Header:
        /// ```
        /// Authorization: Bearer {token}
        /// ```
        /// </remarks>
        /// <param name="cmd">بيانات الدخول — هاتف أو username + باسورد</param>
        /// <response code="200">دخول ناجح — يرجع JWT Token</response>
        /// <response code="401">بيانات دخول غير صحيحة</response>
        /// <response code="403">الحساب مقفل أو معلّق</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiSuccessResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "تسجيل الدخول",
            Description = "Donor يدخل بالهاتف — Staff/Admin يدخل بالـ Username",
            OperationId = "Auth_Login",
            Tags = new[] { "Auth — Public" }
        )]
        public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
            => Ok(await _mediator.Send(cmd));


        // ═══════════════════════════════════════════════════════════════
        //  4. FORGOT PASSWORD — نسيت الباسورد
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// طلب إعادة تعيين الباسورد (Forgot Password)
        /// </summary>
        /// <remarks>
        /// يُرسل **رمز OTP** مكوّن من 6 أرقام إلى الإيميل المُدخل.
        /// الرمز صالح لمدة **10 دقائق**.
        ///
        /// **Request Example:**
        /// ```json
        /// {
        ///   "email": "ahmed@example.com"
        /// }
        /// ```
        ///
        /// **Success Response (200):**
        /// ```json
        /// {
        ///   "succeeded": true,
        ///   "message": "An OTP has been sent to your email address.",
        ///   "data": null
        /// }
        /// ```
        ///
        /// **ملاحظة أمنية:** حتى لو الإيميل غير موجود، الـ Response يبدو نفسه
        /// لمنع enumeration attacks — لكن في المشروع الحالي يُرجع 404.
        /// </remarks>
        /// <param name="cmd">الإيميل المسجّل في النظام</param>
        /// <response code="200">تم إرسال الـ OTP على الإيميل</response>
        /// <response code="404">الإيميل غير مسجّل في النظام</response>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiSuccessResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "طلب إعادة تعيين الباسورد",
            Description = "يُرسل رمز OTP على الإيميل — صالح 10 دقائق",
            OperationId = "Auth_ForgotPassword",
            Tags = new[] { "Auth — Public" }
        )]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand cmd)
            => Ok(await _mediator.Send(cmd));


        // ═══════════════════════════════════════════════════════════════
        //  5. RESET PASSWORD — إعادة تعيين الباسورد
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// إعادة تعيين الباسورد بالـ OTP (Reset Password)
        /// </summary>
        /// <remarks>
        /// يُعيد تعيين الباسورد باستخدام الـ OTP المُرسل من `/forgot-password`.
        ///
        /// **الخطوات:**
        /// 1. اطلب OTP من `/forgot-password`
        /// 2. أدخل الـ OTP + الباسورد الجديد هنا
        ///
        /// **Request Example:**
        /// ```json
        /// {
        ///   "email": "ahmed@example.com",
        ///   "otp": "847291",
        ///   "newPassword": "NewSecurePass456"
        /// }
        /// ```
        ///
        /// **Success Response (200):**
        /// ```json
        /// {
        ///   "succeeded": true,
        ///   "message": "Password has been reset successfully. You can now log in.",
        ///   "data": null
        /// }
        /// ```
        /// </remarks>
        /// <param name="cmd">الإيميل + الـ OTP + الباسورد الجديد</param>
        /// <response code="200">تم تغيير الباسورد بنجاح</response>
        /// <response code="400">الـ OTP منتهي الصلاحية أو غير صحيح، أو الباسورد ضعيف</response>
        /// <response code="404">الإيميل غير موجود</response>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiSuccessResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "إعادة تعيين الباسورد بالـ OTP",
            Description = "يُغيّر الباسورد بعد التحقق من رمز OTP المُرسل بالبريد",
            OperationId = "Auth_ResetPassword",
            Tags = new[] { "Auth — Public" }
        )]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand cmd)
            => Ok(await _mediator.Send(cmd));


        // ═══════════════════════════════════════════════════════════════
        //  6. REGISTER DONOR BY STAFF — تسجيل متبرع من الداشبورد
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// [Reception/Admin] تسجيل متبرع جديد من الداشبورد
        /// </summary>
        /// <remarks>
        /// يُسجّل الموظف متبرعاً جديداً مباشرة من الداشبورد بدون الحاجة لـ OTP.
        /// الحساب يكون **مفعّلاً فوراً** لأن الموظف مسؤول عن التحقق.
        ///
        /// **⚠️ يتطلب Bearer Token** لمستخدم بدور `Reception` أو `Admin`
        ///
        /// **Request Example:**
        /// ```json
        /// {
        ///   "name": "Mona Ali",
        ///   "email": "mona@example.com",
        ///   "phoneNumber": "01198765432",
        ///   "password": "TempPass123",
        ///   "job": "Teacher",
        ///   "landline": "0223456789"
        /// }
        /// ```
        ///
        /// **Success Response (201):**
        /// ```json
        /// {
        ///   "succeeded": true,
        ///   "message": "Donor registered successfully.",
        ///   "data": { "userId": 55, "message": "Donor registered by staff." }
        /// }
        /// ```
        /// </remarks>
        /// <param name="cmd">بيانات المتبرع الجديد</param>
        /// <response code="201">تم تسجيل المتبرع وتفعيل حسابه بنجاح</response>
        /// <response code="401">لم يتم تقديم Token</response>
        /// <response code="403">الـ Token موجود لكن الدور غير مصرّح (ليس Reception أو Admin)</response>
        /// <response code="409">رقم الهاتف مسجّل مسبقاً</response>
        [HttpPost("register-donor")]
        [Authorize(Roles = "Reception,Admin")]
        [ProducesResponseType(typeof(ApiSuccessResponse<RegisterDonorResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        [SwaggerOperation(
            Summary = "[Reception/Admin] تسجيل متبرع من الداشبورد",
            Description = "تسجيل فوري بدون OTP — يتطلب دور Reception أو Admin",
            OperationId = "Auth_RegisterDonorByStaff",
            Tags = new[] { "Auth — Staff & Admin" }
        )]
        public async Task<IActionResult> RegisterDonorByStaff(
            [FromBody] RegisterDonorByStaffCommand cmd)
            => Ok(await _mediator.Send(cmd));


        // ═══════════════════════════════════════════════════════════════
        //  7. CREATE STAFF — إنشاء حساب موظف (Admin فقط)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// [Admin Only] إنشاء حساب موظف جديد
        /// </summary>
        /// <remarks>
        /// يُنشئ الأدمن حساباً لموظف جديد (Reception أو Admin آخر).
        /// الحساب يكون **مفعّلاً فوراً**.
        ///
        /// **⚠️ يتطلب Bearer Token** لمستخدم بدور `Admin` فقط
        ///
        /// **StaffType Values:**
        /// - `1` → Admin
        /// - `2` → Reception
        ///
        /// **Request Example (إنشاء موظف استقبال):**
        /// ```json
        /// {
        ///   "name": "Sara Hassan",
        ///   "username": "reception_sara",
        ///   "email": "sara@resala.org",
        ///   "phoneNumber": "01123456789",
        ///   "password": "StaffPass789",
        ///   "staffType": 2
        /// }
        /// ```
        ///
        /// **Success Response (201):**
        /// ```json
        /// {
        ///   "succeeded": true,
        ///   "message": "Staff account created successfully.",
        ///   "data": {
        ///     "staffId": 3,
        ///     "username": "reception_sara"
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="cmd">بيانات الموظف الجديد — الاسم والـ Username والنوع</param>
        /// <response code="201">تم إنشاء حساب الموظف بنجاح</response>
        /// <response code="401">لم يتم تقديم Token</response>
        /// <response code="403">الـ Token موجود لكن الدور ليس Admin</response>
        /// <response code="409">الـ Username مأخوذ مسبقاً</response>
        /// <response code="400">فشل في إنشاء الحساب (باسورد ضعيف مثلاً)</response>
        [HttpPost("create-staff")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiSuccessResponse<CreateStaffResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "[Admin Only] إنشاء حساب موظف",
            Description = "يُنشئ حساب Reception أو Admin — يتطلب دور Admin فقط",
            OperationId = "Auth_CreateStaff",
            Tags = new[] { "Auth — Staff & Admin" }
        )]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffCommand cmd)
            => Ok(await _mediator.Send(cmd));


        // ═══════════════════════════════════════════════════════════════
        //  Response Model Docs (Swagger schema only — not real classes)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Wrapper for Success Response</summary>
        private record ApiSuccessResponse<T>(bool Succeeded, string Message, T Data);

        /// <summary>Wrapper for Error Response</summary>
        private record ApiErrorResponse(
            bool Succeeded,
            string Message,
            Dictionary<string, string[]>? Errors
        );
    }
}