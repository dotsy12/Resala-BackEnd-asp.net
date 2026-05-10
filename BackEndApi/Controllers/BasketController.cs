using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Basket.Commands.AddBasketItem;
using BackEnd.Application.Features.Basket.Commands.DeleteBasket;
using BackEnd.Application.Features.Basket.Commands.RemoveBasketItem;
using BackEnd.Application.Features.Basket.Queries.GetBasket;
using BackEnd.Domain.Entities.Basket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackEnd.Api.Controllers
{
    [Route("api/v1/basket")]
    [ApiController]
    [Authorize(Roles = "Donor")]
    [Produces("application/json")]
    [SwaggerTag("Basket — سلة التبرعات المؤقتة")]
    public class BasketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BasketController(IMediator mediator)
            => _mediator = mediator;

        private string GetDonorId() => User.FindFirst("donorId")?.Value ?? string.Empty;

        /// <summary>جلب سلة المتبرع الحالي</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<DonorBasket>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] جلب سلة التبرعات",
            Tags = new[] { "Basket — Donor" })]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new GetBasketQuery(GetDonorId()), cancellationToken));

        /// <summary>إضافة عنصر جديد إلى السلة</summary>
        [HttpPost("items")]
        [ProducesResponseType(typeof(ApiResponse<DonorBasket>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] إضافة عنصر إلى السلة",
            Description = "يدعم ItemType بقيم: Sponsorship أو EmergencyCase",
            Tags = new[] { "Basket — Donor" })]
        public async Task<IActionResult> AddItem(
            [FromBody] AddBasketItemRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AddBasketItemCommand(
                GetDonorId(),
                request.TargetId,
                request.ItemType,
                request.Amount,
                request.Title,
                request.ImageUrl);

            return Ok(await _mediator.Send(command, cancellationToken));
        }

        /// <summary>حذف عنصر واحد من السلة</summary>
        [HttpDelete("items/{itemId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DonorBasket>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] حذف عنصر من السلة",
            Tags = new[] { "Basket — Donor" })]
        public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new RemoveBasketItemCommand(GetDonorId(), itemId), cancellationToken));

        /// <summary>حذف السلة بالكامل</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "[Donor] حذف السلة بالكامل",
            Tags = new[] { "Basket — Donor" })]
        public async Task<IActionResult> Delete(CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new DeleteBasketCommand(GetDonorId()), cancellationToken));
    }

    public record AddBasketItemRequest(
        int TargetId,
        string ItemType,
        decimal Amount,
        string Title,
        string? ImageUrl);
}
