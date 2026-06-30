using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Cart;
using BackEnd.Application.Features.Cart.Commands.AddToCart;
using BackEnd.Application.Features.Cart.Commands.RemoveFromCart;
using BackEnd.Application.Features.Cart.Queries.GetCart;
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
    [Route("api/v1/cart")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Roles = "Donor")]
    [SwaggerTag("Cart / Wishlist — سلة التبرعات وقائمة الرغبات")]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private int GetDonorId() =>
            int.TryParse(User.FindFirst("donorId")?.Value, out var id) ? id : 0;

        /// <summary>إضافة كفالة أو حالة طوارئ إلى السلة</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CartItemDto>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "[Donor] إضافة عنصر إلى السلة")]
        public async Task<IActionResult> AddToCart(
            [FromBody] AddToCartRequest request,
            CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            var result = await _mediator.Send(new AddToCartCommand(donorId, request.SponsorshipId, request.EmergencyCaseId), ct);
            return Ok(result);
        }

        /// <summary>حذف عنصر من السلة</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "[Donor] حذف عنصر من السلة")]
        public async Task<IActionResult> RemoveFromCart(
            int id,
            CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            var result = await _mediator.Send(new RemoveFromCartCommand(donorId, id), ct);
            return Ok(result);
        }

        /// <summary>عرض السلة الخاصة بالمتبرع</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CartItemDto>>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "[Donor] عرض عناصر السلة")]
        public async Task<IActionResult> GetCart(CancellationToken ct)
        {
            var donorId = GetDonorId();
            if (donorId == 0)
                return Ok(Result<object>.Failure("لم يتم التعرف على هوية المتبرع.", ErrorType.Unauthorized));

            var result = await _mediator.Send(new GetCartQuery(donorId), ct);
            return Ok(result);
        }
    }

    public class AddToCartRequest
    {
        public int? SponsorshipId { get; set; }
        public int? EmergencyCaseId { get; set; }
    }
}
