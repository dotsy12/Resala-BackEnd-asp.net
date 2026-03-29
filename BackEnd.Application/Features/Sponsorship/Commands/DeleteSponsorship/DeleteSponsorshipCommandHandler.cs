using BackEnd.Application.Common.ResponseFormat;
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
    public class DeleteSponsorshipCommandHandler
    : IRequestHandler<DeleteSponsorshipCommand, Result<bool>>
    {
        private readonly ISponsorshipRepository _repository;

        public DeleteSponsorshipCommandHandler(ISponsorshipRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(
            DeleteSponsorshipCommand request,
            CancellationToken cancellationToken)
        {
            var sponsorship = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (sponsorship is null)
            {
                return Result<bool>.Failure(
                    $"Sponsorship with id {request.Id} not found",
                    ErrorType.NotFound
                );
            }

            await _repository.DeleteAsync(sponsorship, cancellationToken);

            return Result<bool>.Success(true, "Sponsorship deleted successfully");
        }
    }

 }
