using BackEnd.Application.Dtos.Sponsorship;
using BackEnd.Application.Features.Sponsorship.Commands.Create;
using BackEnd.Application.Features.Sponsorship.Commands.DeleteSponsorship;
using BackEnd.Application.Features.Sponsorship.Commands.UpdateSponsorship;
using BackEnd.Application.Features.Sponsorship.Queries.GetAll;
using BackEnd.Application.Features.Sponsorship.Queries.GetById;
using BackEnd.Application.ViewModles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackEndApi.Controllers
{
    [ApiController]
    [Route("api/v1/sponsorships")]
    [Produces("application/json")]
    [SwaggerTag("Sponsorships — إدارة برامج الكفالة والرعاية")]
    public class SponsorshipsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SponsorshipsController(IMediator mediator)
            => _mediator = mediator;

        // ═══════════════════════════════════════════════════
        //  1. CREATE — إنشاء برنامج كفالة جديد
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Admin] إنشاء برنامج كفالة جديد
        /// </summary>
        /// <remarks>
        /// يُنشئ برنامج كفالة جديد في النظام.
        ///
        /// **⚠️ يتطلب Bearer Token بدور Admin**
        ///
        /// **Request Example:**
        /// ```json
        /// {
        ///   "name": "كفالة يتيم",
        ///   "description": "برنامج لكفالة الأيتام وتوفير احتياجاتهم",
        ///   "imageUrl": "https://example.com/image.jpg",
        ///   "icon": "orphan-icon",
        ///   "targetAmount": 50000
        /// }
        /// ```
        ///
        /// **Success Response (201):**
        /// ```json
        /// {
        ///   "id": 1,
        ///   "name": "كفالة يتيم",
        ///   "description": "برنامج لكفالة الأيتام",
        ///   "targetAmount": 50000,
        ///   "collectedAmount": 0,
        ///   "isActive": true,
        ///   "createdAt": "2026-03-21T00:00:00"
        /// }
        /// ```
        /// </remarks>
        /// <response code="201">تم إنشاء برنامج الكفالة بنجاح</response>
        /// <response code="400">بيانات غير صحيحة</response>
        /// <response code="401">لم يتم تقديم Token</response>
        /// <response code="403">الدور غير مصرّح</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(SponsorshipViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Admin] إنشاء برنامج كفالة جديد",
            Description = "يُنشئ برنامج كفالة — يتطلب دور Admin",
            OperationId = "Sponsorships_Create",
            Tags = new[] { "Sponsorships — Admin" }
        )]
        public async Task<ActionResult<SponsorshipViewModel>> Create(
            [FromBody] CreateSponsorshipDto dto,
            CancellationToken cancellationToken)
        {
            var command = new CreateSponsorshipCommand(dto);
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // ═══════════════════════════════════════════════════
        //  2. GET ALL — جلب كل برامج الكفالة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// جلب كل برامج الكفالة
        /// </summary>
        /// <remarks>
        /// يرجع قائمة بكل برامج الكفالة المتاحة في النظام.
        ///
        /// **متاح للجميع بدون Token**
        ///
        /// **Success Response (200):**
        /// ```json
        /// [
        ///   {
        ///     "id": 1,
        ///     "name": "كفالة يتيم",
        ///     "description": "برنامج لكفالة الأيتام",
        ///     "targetAmount": 50000,
        ///     "collectedAmount": 12000,
        ///     "isActive": true,
        ///     "createdAt": "2026-03-21T00:00:00"
        ///   }
        /// ]
        /// ```
        /// </remarks>
        /// <response code="200">قائمة برامج الكفالة</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<SponsorshipViewModel>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "جلب كل برامج الكفالة",
            Description = "يرجع قائمة كاملة بكل برامج الكفالة — متاح للجميع",
            OperationId = "Sponsorships_GetAll",
            Tags = new[] { "Sponsorships — Public" }
        )]
        public async Task<ActionResult<IEnumerable<SponsorshipViewModel>>> GetAll(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllSponsorshipsQuery(), cancellationToken);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════
        //  3. GET BY ID — جلب برنامج كفالة بالـ ID
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// جلب برنامج كفالة بالـ ID
        /// </summary>
        /// <remarks>
        /// يرجع تفاصيل برنامج كفالة محدد.
        ///
        /// **متاح للجميع بدون Token**
        ///
        /// **Success Response (200):**
        /// ```json
        /// {
        ///   "id": 1,
        ///   "name": "كفالة يتيم",
        ///   "description": "برنامج لكفالة الأيتام",
        ///   "imageUrl": "https://example.com/image.jpg",
        ///   "icon": "orphan-icon",
        ///   "targetAmount": 50000,
        ///   "collectedAmount": 12000,
        ///   "isActive": true,
        ///   "createdAt": "2026-03-21T00:00:00"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">رقم برنامج الكفالة</param>
        /// <response code="200">تفاصيل برنامج الكفالة</response>
        /// <response code="404">برنامج الكفالة غير موجود</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SponsorshipViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "جلب برنامج كفالة بالـ ID",
            Description = "يرجع تفاصيل برنامج كفالة محدد",
            OperationId = "Sponsorships_GetById",
            Tags = new[] { "Sponsorships — Public" }
        )]
        public async Task<ActionResult<SponsorshipViewModel>> GetById(
            int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSponsorshipByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════
        //  4. UPDATE — تعديل برنامج كفالة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Admin] تعديل برنامج كفالة
        /// </summary>
        /// <remarks>
        /// يُعدّل بيانات برنامج كفالة موجود.
        ///
        /// **⚠️ يتطلب Bearer Token بدور Admin**
        ///
        /// **Request Example:**
        /// ```json
        /// {
        ///   "name": "كفالة يتيم — محدّث",
        ///   "description": "وصف محدّث للبرنامج",
        ///   "imageUrl": "https://example.com/new-image.jpg",
        ///   "icon": "new-icon",
        ///   "targetAmount": 75000,
        ///   "isActive": true
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">رقم برنامج الكفالة</param>
        /// <response code="200">تم التعديل بنجاح</response>
        /// <response code="400">بيانات غير صحيحة</response>
        /// <response code="404">برنامج الكفالة غير موجود</response>
        /// <response code="401">لم يتم تقديم Token</response>
        /// <response code="403">الدور غير مصرّح</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(SponsorshipViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Admin] تعديل برنامج كفالة",
            Description = "يُعدّل بيانات برنامج كفالة — يتطلب دور Admin",
            OperationId = "Sponsorships_Update",
            Tags = new[] { "Sponsorships — Admin" }
        )]
        public async Task<ActionResult<SponsorshipViewModel>> Update(
            int id,
            [FromBody] UpdateSponsorshipDto dto,
            CancellationToken cancellationToken)
        {
            var command = new UpdateSponsorshipCommand(id, dto);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════
        //  5. DELETE — حذف برنامج كفالة
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// [Admin] حذف برنامج كفالة
        /// </summary>
        /// <remarks>
        /// يحذف برنامج كفالة من النظام.
        ///
        /// **⚠️ يتطلب Bearer Token بدور Admin**
        ///
        /// **ملاحظة:** الحذف نهائي — لا يمكن التراجع.
        /// </remarks>
        /// <param name="id">رقم برنامج الكفالة المراد حذفه</param>
        /// <response code="204">تم الحذف بنجاح</response>
        /// <response code="404">برنامج الكفالة غير موجود</response>
        /// <response code="401">لم يتم تقديم Token</response>
        /// <response code="403">الدور غير مصرّح</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "[Admin] حذف برنامج كفالة",
            Description = "يحذف برنامج كفالة نهائياً — يتطلب دور Admin",
            OperationId = "Sponsorships_Delete",
            Tags = new[] { "Sponsorships — Admin" }
        )]
        public async Task<IActionResult> Delete(
            int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteSponsorshipCommand(id), cancellationToken);
            return NoContent();
        }
    }
}