using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Staff;
using BackEnd.Application.Features.Admin.Staff.Commands.DeleteStaff;
using BackEnd.Application.Features.Admin.Staff.Queries.GetAllStaff;
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
    [Route("api/v1/admin/staff")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Staff Management — إدارة حسابات الموظفين (Admin Only)")]
    public class StaffController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StaffController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// [Admin] جلب كل حسابات الموظفين (مع pagination وبحث)
        /// </summary>
        /// <remarks>
        /// يرجع قائمة بجميع الموظفين المسجلين في النظام (غير المحذوفين).
        /// يمكن البحث بالاسم، الإيميل، أو اسم المستخدم.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(Result<PagedResult<StaffDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] جلب كل حسابات الموظفين",
            Description = "يرجع قائمة مرقمة للموظفين مع إمكانية البحث.",
            OperationId = "Staff_GetAll",
            Tags = new[] { "Staff Management" }
        )]
        public async Task<ActionResult<Result<PagedResult<StaffDto>>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var query = new GetAllStaffQuery(search, pageNumber, pageSize);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] حذف حساب موظف (Soft Delete)
        /// </summary>
        /// <remarks>
        /// يقوم بتعطيل حساب الموظف ومنعه من الدخول، مع الاحتفاظ ببياناته في قاعدة البيانات (حذف منطقي).
        /// </remarks>
        /// <param name="id">رقم الموظف (StaffId)</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Admin] حذف حساب موظف",
            Description = "يعطل حساب الموظف ويمنعه من الوصول (Soft Delete).",
            OperationId = "Staff_Delete",
            Tags = new[] { "Staff Management" }
        )]
        public async Task<ActionResult<Result<bool>>> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteStaffCommand(id), ct);
            if (!result.IsSuccess && result.ErrorType == ErrorType.NotFound)
                return NotFound(result);
                
            return Ok(result);
        }
    }
}
