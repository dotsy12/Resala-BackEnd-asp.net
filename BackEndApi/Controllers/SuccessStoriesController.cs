using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.SuccessStory;
using BackEnd.Application.Features.SuccessStories.Commands.CreateSuccessStory;
using BackEnd.Application.Features.SuccessStories.Commands.DeleteSuccessStory;
using BackEnd.Application.Features.SuccessStories.Queries.GetAllSuccessStories;
using BackEnd.Application.Features.SuccessStories.Queries.GetSuccessStoryById;
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
    [SwaggerTag("Success Stories — إدارة قصص النجاح")]
    public class SuccessStoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuccessStoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// جلب كل قصص النجاح (عام)
        /// </summary>
        [HttpGet("api/v1/success-stories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Result<IReadOnlyList<SuccessStoryDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "جلب كل قصص النجاح",
            Description = "يرجع قائمة بكل قصص النجاح المنشورة مرتبة من الأحدث إلى الأقدم.",
            OperationId = "SuccessStories_GetAll",
            Tags = new[] { "Success Stories — Public" }
        )]
        public async Task<ActionResult<Result<IReadOnlyList<SuccessStoryDto>>>> GetAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllSuccessStoriesQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// جلب تفاصيل قصة نجاح (عام)
        /// </summary>
        [HttpGet("api/v1/success-stories/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Result<SuccessStoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<SuccessStoryDto>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "جلب تفاصيل قصة نجاح",
            Description = "يرجع تفاصيل قصة نجاح معينة بناءً على المعرف.",
            OperationId = "SuccessStories_GetById",
            Tags = new[] { "Success Stories — Public" }
        )]
        public async Task<ActionResult<Result<SuccessStoryDto>>> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetSuccessStoryByIdQuery(id), ct);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] إضافة قصة نجاح جديدة
        /// </summary>
        [HttpPost("api/v1/admin/success-stories")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Result<SuccessStoryDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Admin] إضافة قصة نجاح",
            Description = "ينشئ قصة نجاح جديدة مع رفع صورة.",
            OperationId = "SuccessStories_Create",
            Tags = new[] { "Success Stories — Admin" }
        )]
        public async Task<ActionResult<Result<SuccessStoryDto>>> Create(
            [FromForm] string title, 
            [FromForm] string description, 
            IFormFile image, 
            CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateSuccessStoryCommand(title, description, image), ct);
            return Ok(result);
        }

        /// <summary>
        /// [Admin] حذف قصة نجاح
        /// </summary>
        [HttpDelete("api/v1/admin/success-stories/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "[Admin] حذف قصة نجاح",
            Description = "يحذف قصة نجاح نهائياً مع حذف صورتها من السيرفر.",
            OperationId = "SuccessStories_Delete",
            Tags = new[] { "Success Stories — Admin" }
        )]
        public async Task<ActionResult<Result<bool>>> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteSuccessStoryCommand(id), ct);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }
    }
}
