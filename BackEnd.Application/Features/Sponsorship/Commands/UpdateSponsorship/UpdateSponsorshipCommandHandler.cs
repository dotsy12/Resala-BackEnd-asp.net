using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Sponsorship.Commands.UpdateSponsorship
{
    public class UpdateSponsorshipCommandHandler : IRequestHandler<UpdateSponsorshipCommand, SponsorshipViewModel>

    {
        private readonly ISponsorshipRepository _repository;

        public UpdateSponsorshipCommandHandler(ISponsorshipRepository repository)
        {
            _repository = repository;
        }

        public async Task<SponsorshipViewModel> Handle(
            UpdateSponsorshipCommand request,
            CancellationToken cancellationToken)
        {
            var sponsorship = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (sponsorship is null)
                throw new SponsorshipNotActiveException(request.Id);

            var dto = request.Dto;

            Money? goal = null;

            if (dto.TargetAmount.HasValue)
                goal = new Money(dto.TargetAmount.Value);

            sponsorship.UpdateImages(dto.ImageUrl, dto.Icon);

            sponsorship.UpdatePolicy(
                sponsorship.Policy
            );

            if (dto.IsActive)
                sponsorship.Activate();
            else
                sponsorship.Deactivate();

            await _repository.UpdateAsync(sponsorship, cancellationToken);

            return new SponsorshipViewModel
            {
                Id = sponsorship.Id,
                Name = sponsorship.Name,
                Description = sponsorship.Description,
                ImageUrl = sponsorship.ImagePath ?? "",
                Icon = sponsorship.IconPath ?? "",
                TargetAmount = sponsorship.FinancialGoal?.Amount,
                CollectedAmount = sponsorship.TotalCollected.Amount,
                IsActive = sponsorship.IsActive,
                CreatedAt = sponsorship.CreatedOn
            };
        }
    }
}
