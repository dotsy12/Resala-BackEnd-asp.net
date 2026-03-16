using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Sponsorship.Commands.DeleteSponsorship
{
    public class DeleteSponsorshipCommandHandler : IRequestHandler<DeleteSponsorshipCommand, Unit>

    {
        private readonly ISponsorshipRepository _repository;

        public DeleteSponsorshipCommandHandler(ISponsorshipRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(
            DeleteSponsorshipCommand request,
            CancellationToken cancellationToken)
        {
            var sponsorship = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (sponsorship is null)
                throw new SponsorshipNotActiveException(request.Id);

            await _repository.DeleteAsync(sponsorship, cancellationToken);

            return Unit.Value;
        }
    }
    }
