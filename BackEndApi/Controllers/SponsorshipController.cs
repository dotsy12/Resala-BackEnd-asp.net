using BackEnd.Application.Dtos.Sponsorship;
using BackEnd.Application.Features.Sponsorship.Commands.Create;
using BackEnd.Application.Features.Sponsorship.Commands.DeleteSponsorship;
using BackEnd.Application.Features.Sponsorship.Commands.UpdateSponsorship;
using BackEnd.Application.Features.Sponsorship.Queries.GetAll;
using BackEnd.Application.Features.Sponsorship.Queries.GetById;
using BackEnd.Application.ViewModles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEndApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorshipsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SponsorshipsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<SponsorshipViewModel>> Create(
            [FromBody] CreateSponsorshipDto dto,
            CancellationToken cancellationToken)
        {
            var command = new CreateSponsorshipCommand(dto);

            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SponsorshipViewModel>>> GetAll(CancellationToken cancellationToken)

        {
            var result = await _mediator.Send(new GetAllSponsorshipsQuery(), cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SponsorshipViewModel>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSponsorshipByIdQuery(id), cancellationToken);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SponsorshipViewModel>> Update(int id, [FromBody] UpdateSponsorshipDto dto, CancellationToken cancellationToken)
        {
            var command = new UpdateSponsorshipCommand(id, dto);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteSponsorshipCommand(id), cancellationToken);

            return NoContent();
        }


    }
}
