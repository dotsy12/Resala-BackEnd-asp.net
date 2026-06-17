using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Features.Admin.DirectOperations.Commands.DirectSubscribe;
using BackEnd.Application.Features.Admin.DirectOperations.Commands.ProcessDirectPayment;
using BackEnd.Application.Features.Admin.DirectOperations.Queries.SearchDonors;
using BackEnd.Application.Features.Subscriptions.Queries.GetMySubscriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace BackEnd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/direct-operations")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,Reception")]
    [SwaggerTag("Direct Branch Operations — عمليات الفرع المباشرة (Staff Only)")]
    public class AdminDirectOperationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminDirectOperationsController(IMediator mediator) => _mediator = mediator;

        private int GetStaffId() => int.TryParse(User.FindFirst("staffId")?.Value, out var id) ? id : 0;

        /// <summary>البحث عن المتبرعين (Name/Phone/Email)</summary>
        [HttpGet("search-donors")]
        [SwaggerOperation(Summary = "[Staff] البحث عن متبرع", Description = "البحث بالاسم أو الهاتف أو البريد الإلكتروني")]
        public async Task<IActionResult> SearchDonors([FromQuery] string query, [FromQuery] int page = 1)
            => Ok(await _mediator.Send(new SearchDonorsQuery(query, page)));

        /// <summary>جلب اشتراكات المتبرع النشطة</summary>
        [HttpGet("donors/{donorId:int}/sponsorships")]
        [SwaggerOperation(Summary = "[Staff] جلب كفالات المتبرع", Description = "يرجع قائمة باشتراكات المتبرع الحالية")]
        public async Task<IActionResult> GetDonorSponsorships(int donorId)
            => Ok(await _mediator.Send(new GetMySubscriptionsQuery(donorId)));

        /// <summary>تحصيل دفعة مباشرة في الفرع</summary>
        [HttpPost("process-payment")]
        [SwaggerOperation(Summary = "[Staff] تحصيل دفعة مباشرة", Description = "تسجيل وتأكيد دفعة فورية لاشتراك أو حالة حرجة")]
        public async Task<IActionResult> ProcessDirectPayment([FromBody] DirectPaymentRequest dto)
        {
            var staffId = GetStaffId();
            if (staffId == 0) return Unauthorized();

            var command = new ProcessDirectPaymentCommand(
                dto.DonorId,
                staffId,
                dto.Amount,
                dto.SubscriptionId,
                dto.EmergencyCaseId,
                dto.GeneralDonationId
            );

            return Ok(await _mediator.Send(command));
        }

        /// <summary>اشتراك مباشر لمحر في برنامج كفالة</summary>
        [HttpPost("direct-subscribe")]
        [SwaggerOperation(Summary = "[Staff] اشتراك مباشر في كفالة", Description = "إنشاء اشتراك جديد للمتبرع في برنامج كفالة")]
        public async Task<IActionResult> DirectSubscribe([FromBody] DirectSubscribeRequest dto)
        {
            var staffId = GetStaffId();
            if (staffId == 0) return Unauthorized();

            var command = new DirectSubscribeCommand(
                dto.DonorId,
                dto.SponsorshipId,
                dto.Amount,
                dto.Cycle,
                staffId
            );

            return Ok(await _mediator.Send(command));
        }
    }

    public record DirectPaymentRequest(
        int DonorId,
        decimal Amount,
        int? SubscriptionId = null,
        int? EmergencyCaseId = null,
        int? GeneralDonationId = null
    );

    public record DirectSubscribeRequest(
        int DonorId,
        int SponsorshipId,
        decimal Amount,
        BackEnd.Domain.Enums.PaymentCycle Cycle
    );
}