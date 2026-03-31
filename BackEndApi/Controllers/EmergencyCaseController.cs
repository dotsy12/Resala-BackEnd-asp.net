using BackEnd.Application.Dtos.EmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Commands.DeleteEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Commands.UpdateEmergencyCase;
using BackEnd.Application.Features.EmergencyCase.Queries.GetAllEmergenciesCasies;
using BackEnd.Application.Features.EmergencyCase.Queries.GetEmergencyCaseById;
using BackEnd.Application.ViewModles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<ActionResult<EmergencyCaseViewModel>> Create(
            [FromBody] CreateEmergencyCaseDto dto,
            CancellationToken cancellationToken)
        {
            var command = new CreateEmergencyCaseCommand(dto);
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
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
        public async Task<ActionResult<EmergencyCaseViewModel>> Update(
            int id,
            [FromBody] UpdateEmergencyCaseDto dto,
            CancellationToken cancellationToken)
        {
            var command = new UpdateEmergencyCaseCommand(id, dto);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Admin] حذف حالة حرجة",
            Description = "حذف حالة حرجة من النظام — يتطلب Admin",
            OperationId = "EmergencyCases_Delete",
            Tags = new[] { "EmergencyCases — Admin" }
        )]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteEmergencyCaseCommand(id), cancellationToken);
            return NoContent();
        }
    }
}