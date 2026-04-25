using BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Commands.DeleteEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Commands.UpdateEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Queries.GetAllEmergenciesCasies;
using BackEnd.Application.Features.EmergencyCase.Queries.GetEmergencyCaseById;
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

        // ═══════════════════════════════════════════════════
        //  1. CREATE
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Admin] إنشاء حالة حرجة جديدة
        /// </summary>
        /// <remarks>
        /// يُنشئ حالة حرجة جديدة في النظام.
        ///
        /// **⚠️ يتطلب Bearer Token بدور Admin**
        ///
        /// **Request Example:**
        /// ```json
        /// {
        ///   "title": "عملية عاجلة",
        ///   "description": "مريض يحتاج عملية فورًا",
        ///   "targetAmount": 50000
        /// }
        /// ```
        ///
        /// **Success Response (201):**
        /// ```json
        /// {
        ///   "id": 1,
        ///   "title": "عملية عاجلة",
        ///   "description": "مريض يحتاج عملية فورًا",
        ///   "targetAmount": 50000,
        ///   "collectedAmount": 0,
        ///   "createdOn": "2026-03-25T00:00:00"
        /// }
        /// ```
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(EmergencyCaseViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Admin] إنشاء حالة حرجة",
            Description = "إنشاء حالة حرجة جديدة — يتطلب Admin",
            OperationId = "EmergencyCases_Create",
            Tags = new[] { "EmergencyCases — Admin" }
        )]
        public async Task<IActionResult> Create(
    [FromForm] CreateEmergencyCaseRequest request,
    CancellationToken ct)
        {
            var cmd = new CreateEmergencyCaseCommand(
                Title: request.Title,
                Description: request.Description,
                UrgencyLevel: request.UrgencyLevel,
                RequiredAmount: request.RequiredAmount,
                Attachment: request.Attachment
            );

            return Ok(await _mediator.Send(cmd, ct));
        }

        // ═══════════════════════════════════════════════════
        //  2. GET ALL
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// جلب كل الحالات الحرجة
        /// </summary>
        /// <remarks>
        /// يرجع قائمة بكل الحالات الحرجة المسجلة في النظام.
        /// </remarks>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<EmergencyCaseViewModel>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "جلب كل الحالات الحرجة",
            Description = "عرض جميع الحالات الحرجة المتاحة",
            OperationId = "EmergencyCases_GetAll",
            Tags = new[] { "EmergencyCases — Public" }
        )]
        public async Task<ActionResult<IEnumerable<EmergencyCaseViewModel>>> GetAll(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllEmergencyCasesQuery(), cancellationToken);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════
        //  3. GET BY ID
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// جلب حالة حرجة بالـ ID
        /// </summary>
        /// <param name="id">رقم الحالة الحرجة</param>
        /// <response code="200">تفاصيل الحالة</response>
        /// <response code="404">الحالة غير موجودة</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EmergencyCaseViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "جلب حالة حرجة بالـ ID",
            Description = "عرض تفاصيل حالة حرجة باستخدام المعرف",
            OperationId = "EmergencyCases_GetById",
            Tags = new[] { "EmergencyCases — Public" }
        )]
        public async Task<ActionResult<EmergencyCaseViewModel>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEmergencyCaseByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════
        //  4. UPDATE
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Admin] تعديل حالة حرجة
        /// </summary>
        /// <param name="id">رقم الحالة الحرجة</param>
        /// <remarks>
        /// تعديل بيانات حالة حرجة موجودة.
        ///
        /// **⚠️ يتطلب Bearer Token بدور Admin**
        /// </remarks>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(EmergencyCaseViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Admin] تعديل حالة حرجة",
            Description = "تعديل بيانات حالة حرجة — يتطلب Admin",
            OperationId = "EmergencyCases_Update",
            Tags = new[] { "EmergencyCases — Admin" }
        )]
        public async Task<IActionResult> Update(
    int id,
    [FromForm] UpdateEmergencyCaseRequest request,
    CancellationToken ct)
        {
            var cmd = new UpdateEmergencyCaseCommand(
                Id: id,
                Title: request.Title,
                Description: request.Description,
                UrgencyLevel: request.UrgencyLevel,
                RequiredAmount: request.RequiredAmount,
                Attachment: request.Attachment,
                IsActive: request.IsActive
            );

            return Ok(await _mediator.Send(cmd, ct));
        }

        // ═══════════════════════════════════════════════════
        //  5. DELETE
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Admin] حذف حالة حرجة
        /// </summary>
        /// <param name="id">رقم الحالة الحرجة</param>
        /// <response code="204">تم الحذف بنجاح</response>
        /// <response code="404">الحالة غير موجودة</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Admin] حذف حالة حرجة",
            Description = "حذف (تعطيل) حالة حرجة من النظام — يتطلب Admin. " +
                          "لا يمكن حذف حالة لديها تبرعات.",
            OperationId = "EmergencyCases_Delete",
            Tags = new[] { "EmergencyCases — Admin" }
        )]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            // ✅ FIX: نُعيد النتيجة — ResultActionFilter يتولى الـ status code
            var result = await _mediator.Send(new DeleteEmergencyCaseCommand(id), cancellationToken);
            return Ok(result);
        }
    }

    public record CreateEmergencyCaseRequest(
        string Title,
        string Description,
        UrgencyLevel UrgencyLevel,
        decimal RequiredAmount,
        IFormFile? Attachment
    );

    public record UpdateEmergencyCaseRequest(
        string Title,
        string Description,
        UrgencyLevel UrgencyLevel,
        decimal? RequiredAmount,
        IFormFile? Attachment,
        bool IsActive
    );
}