using BackEnd.Application.Features.InKindDonation.Commands.CreateInKindDonation;
using BackEnd.Application.Features.InKindDonation.Commands.DeleteInKindDonation;
using BackEnd.Application.Features.InKindDonation.Commands.UpdateInKindDonation;
using BackEnd.Application.Features.InKindDonation.Queries.GetAllInKindDonationsQuery;
using BackEnd.Application.Features.InKindDonation.Queries.GetDonorInKindDonationsQuery;
using BackEnd.Application.Features.InKindDonation.Queries.GetInKindDonationByIdQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace BackEnd.Api.Controllers
{
    [Route("api/v1/in-kind-donations")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Roles = "Reception,Admin")]
    [SwaggerTag("InKindDonations — إدارة التبرعات العينية")]
    public class InKindDonationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InKindDonationController(IMediator mediator)
            => _mediator = mediator;

        // ═══════════════════════════════════════════════════
        //  Helper — جلب StaffId من الـ JWT Token
        // ═══════════════════════════════════════════════════
        private int GetStaffIdFromToken()
        {
            var staffIdClaim = User.FindFirst("staffId")?.Value;
            return int.TryParse(staffIdClaim, out var id) ? id : 0;
        }

        // ═══════════════════════════════════════════════════
        //  1. RECORD — تسجيل تبرع عيني جديد
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Reception/Admin] تسجيل تبرع عيني جديد
        /// </summary>
        /// <remarks>
        /// يُسجّل الموظف تبرعاً عينياً لمتبرع موجود في النظام.
        ///
        /// **⚠️ يتطلب Bearer Token بدور Reception أو Admin**
        ///
        /// الـ `recordedByStaffId` يُحدَّد تلقائياً من الـ Token — لا تُرسله.
        ///
        /// **Request Example:**
        /// ```json
        /// {
        ///   "donorId": 5,
        ///   "donationTypeName": "ملابس",
        ///   "quantity": 10,
        ///   "description": "ملابس شتوية للأطفال"
        /// }
        /// ```
        ///
        /// **Success Response (200):**
        /// ```json
        /// {
        ///   "succeeded": true,
        ///   "data": {
        ///     "id": 1,
        ///     "donorId": 5,
        ///     "donorName": "Ahmed Mohamed",
        ///     "donationTypeName": "ملابس",
        ///     "quantity": 10,
        ///     "description": "ملابس شتوية للأطفال",
        ///     "recordedByStaffId": 2,
        ///     "recordedByStaffName": "Sara Hassan",
        ///     "recordedAt": "2026-03-25T00:00:00",
        ///     "createdOn": "2026-03-25T00:00:00"
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">تم تسجيل التبرع بنجاح</response>
        /// <response code="400">بيانات غير صحيحة</response>
        /// <response code="404">المتبرع غير موجود</response>
        /// <response code="401">لم يتم تقديم Token</response>
        /// <response code="403">الدور غير مصرّح</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "[Reception/Admin] تسجيل تبرع عيني جديد",
            Description = "يُسجّل تبرعاً عينياً لمتبرع — يتطلب Reception أو Admin",
            OperationId = "InKindDonations_Record",
            Tags = new[] { "InKindDonations — Staff" }
        )]
        public async Task<IActionResult> Record(
            [FromBody] RecordInKindDonationRequest request,
            CancellationToken ct)
        {
            var staffId = GetStaffIdFromToken();
            if (staffId == 0)
                return Unauthorized();

            var cmd = new RecordInKindDonationCommand(
                DonorId: request.DonorId,
                DonationTypeName: request.DonationTypeName,
                Quantity: request.Quantity,
                Description: request.Description,
                RecordedByStaffId: staffId
            );

            return Ok(await _mediator.Send(cmd, ct));
        }

        // ═══════════════════════════════════════════════════
        //  2. GET ALL — جلب كل التبرعات العينية
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Reception/Admin] جلب كل التبرعات العينية
        /// </summary>
        /// <remarks>
        /// يرجع قائمة بكل التبرعات العينية المسجّلة في النظام مرتبة من الأحدث.
        ///
        /// **⚠️ يتطلب Bearer Token بدور Reception أو Admin**
        /// </remarks>
        /// <response code="200">قائمة التبرعات العينية</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "[Reception/Admin] جلب كل التبرعات العينية",
            OperationId = "InKindDonations_GetAll",
            Tags = new[] { "InKindDonations — Staff" }
        )]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAllInKindDonationsQuery(), ct));

        // ═══════════════════════════════════════════════════
        //  3. GET BY ID — جلب تبرع عيني بالـ ID
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Reception/Admin] جلب تبرع عيني بالـ ID
        /// </summary>
        /// <param name="id">رقم التبرع العيني</param>
        /// <response code="200">تفاصيل التبرع</response>
        /// <response code="404">التبرع غير موجود</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "[Reception/Admin] جلب تبرع عيني بالـ ID",
            OperationId = "InKindDonations_GetById",
            Tags = new[] { "InKindDonations — Staff" }
        )]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => Ok(await _mediator.Send(new GetInKindDonationByIdQuery(id), ct));

        // ═══════════════════════════════════════════════════
        //  4. GET BY DONOR — جلب تبرعات متبرع معين
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Reception/Admin] جلب كل التبرعات العينية لمتبرع محدد
        /// </summary>
        /// <param name="donorId">رقم المتبرع</param>
        /// <response code="200">قائمة تبرعات المتبرع</response>
        /// <response code="404">المتبرع غير موجود</response>
        [HttpGet("donor/{donorId:int}")]
        [SwaggerOperation(
            Summary = "[Reception/Admin] جلب تبرعات متبرع معين",
            OperationId = "InKindDonations_GetByDonor",
            Tags = new[] { "InKindDonations — Staff" }
        )]
        public async Task<IActionResult> GetByDonor(int donorId, CancellationToken ct)
            => Ok(await _mediator.Send(new GetDonorInKindDonationsQuery(donorId), ct));

        // ═══════════════════════════════════════════════════
        //  5. UPDATE — تعديل تبرع عيني
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Reception/Admin] تعديل تبرع عيني
        /// </summary>
        /// <param name="id">رقم التبرع العيني</param>
        /// <response code="200">تم التعديل بنجاح</response>
        /// <response code="400">بيانات غير صحيحة</response>
        /// <response code="404">التبرع غير موجود</response>
        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "[Reception/Admin] تعديل تبرع عيني",
            OperationId = "InKindDonations_Update",
            Tags = new[] { "InKindDonations — Staff" }
        )]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateInKindDonationRequest request,
            CancellationToken ct)
        {
            var cmd = new UpdateInKindDonationCommand(
                Id: id,
                DonationTypeName: request.DonationTypeName,
                Quantity: request.Quantity,
                Description: request.Description
            );
            return Ok(await _mediator.Send(cmd, ct));
        }

        // ═══════════════════════════════════════════════════
        //  6. DELETE — حذف تبرع عيني
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Admin Only] حذف تبرع عيني
        /// </summary>
        /// <param name="id">رقم التبرع العيني</param>
        /// <response code="200">تم الحذف بنجاح</response>
        /// <response code="404">التبرع غير موجود</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "[Admin Only] حذف تبرع عيني",
            OperationId = "InKindDonations_Delete",
            Tags = new[] { "InKindDonations — Admin" }
        )]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
            => Ok(await _mediator.Send(new DeleteInKindDonationCommand(id), ct));
    }

    // ═══════════════════════════════════════════════════
    //  Request Models (بدل ما تبعت StaffId في الـ Body)
    // ═══════════════════════════════════════════════════

    public record RecordInKindDonationRequest(
        int DonorId,
        string DonationTypeName,
        int Quantity,
        string? Description
    );

    public record UpdateInKindDonationRequest(
        string DonationTypeName,
        int Quantity,
        string? Description
    );
}
