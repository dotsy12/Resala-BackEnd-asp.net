using AutoMapper;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Sponsorship;
using BackEnd.Application.Features.Sponsorship.Commands.Create;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.ValueObjects;
using BackEnd.Domain.Entities.Sponsorship;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Sponsorship.Commands.Create
{
    public class CreateSponsorshipCommandHandler
       : IRequestHandler<CreateSponsorshipCommand, Result<SponsorshipViewModel>>
    {
        private readonly ISponsorshipRepository _repository;

        public CreateSponsorshipCommandHandler(ISponsorshipRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<SponsorshipViewModel>> Handle(
            CreateSponsorshipCommand request,
            CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            Money? goal = null;

            if (dto.TargetAmount.HasValue)
                goal = new Money(dto.TargetAmount.Value);

            var sponsorship = BackEnd.Domain.Entities.Sponsorship.Sponsorship.Create(
                dto.Name,
                dto.Description,
                dto.Icon,
                goal
            );

            var created = await _repository.CreateAsync(sponsorship, cancellationToken);

            var viewModel = new SponsorshipViewModel
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                ImageUrl = created.ImagePath ?? "",
                Icon = created.IconPath ?? "",
                TargetAmount = created.FinancialGoal?.Amount,
                CollectedAmount = created.TotalCollected.Amount,
                IsActive = created.IsActive,
                CreatedAt = created.CreatedOn
            };

            return Result<SponsorshipViewModel>.Success(viewModel, "Sponsorship created successfully");
        }
    }

}
