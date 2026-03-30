using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Sponsorship.Queries.GetById;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Commands.UpdateEmergencyCase
{

    public class UpdateEmergencyCaseCommandHandler
      : IRequestHandler<UpdateEmergencyCaseCommand, Result<EmergencyCaseViewModel>>
    {
        private readonly IEmergencyCaseRepository _repository;

        public UpdateEmergencyCaseCommandHandler(IEmergencyCaseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<EmergencyCaseViewModel>> Handle(
            UpdateEmergencyCaseCommand request,
            CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null)
            {
                return Result<EmergencyCaseViewModel>.Failure(
                    "Emergency case not found",
                    ErrorType.NotFound);
            }

            var dto = request.Dto;

            // Update basic info
            entity.UpdateDetails(dto.Title, dto.Description, dto.ImageUrl);

            // Update urgency
            entity.SetUrgency(dto.UrgencyLevel);

            // Update RequiredAmount
            if (dto.RequiredAmount.HasValue)
            {
                if (dto.RequiredAmount.Value <= 0)
                {
                    return Result<EmergencyCaseViewModel>.Failure(
                        "Required amount must be greater than zero",
                        ErrorType.BadRequest);
                }

                var money = new Money(dto.RequiredAmount.Value, "EGP");
                entity.UpdateRequiredAmount(money);
            }

            // Activate / Deactivate
            if (dto.IsActive)
                entity.Activate();
            else
                entity.Deactivate();

            // Save
            await _repository.UpdateAsync(entity, cancellationToken);

            // Map
            var vm = new EmergencyCaseViewModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                ImageUrl = entity.ImagePath ?? string.Empty,
                UrgencyLevel = entity.UrgencyLevel.ToString(),
                RequiredAmount = entity.RequiredAmount.Amount,
                CollectedAmount = entity.CollectedAmount.Amount,
                IsActive = entity.IsActive,
                IsCompleted = entity.IsCompleted,
                CreatedAt = entity.CreatedOn
            };

            return Result<EmergencyCaseViewModel>.Success(vm, "Updated successfully");
        }
    }
}

