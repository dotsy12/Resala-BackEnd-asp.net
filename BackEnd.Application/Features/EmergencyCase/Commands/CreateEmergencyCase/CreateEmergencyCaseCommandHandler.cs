using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackEnd.Domain.Entities;
using BackEnd.Domain.Entities.EmergencyCase;

namespace BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase
{
    public class CreateEmergencyCaseCommandHandler
      : IRequestHandler<CreateEmergencyCaseCommand, Result<EmergencyCaseViewModel>>
    {
        private readonly IEmergencyCaseRepository _repository;

        public CreateEmergencyCaseCommandHandler(IEmergencyCaseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<EmergencyCaseViewModel>> Handle(
            CreateEmergencyCaseCommand request,
            CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            // ✅ Value Object
            var requiredAmount = new Money(dto.RequiredAmount);

            // ✅ Create Domain Entity
            var entity = BackEnd.Domain.Entities.EmergencyCase.EmergencyCase.Create(
                dto.Title,
                dto.Description,
                dto.UrgencyLevel,
                requiredAmount,
                dto.ImageUrl
            );

            var created = await _repository.CreateAsync(entity, cancellationToken);

            // ✅ Mapping
            var viewModel = new EmergencyCaseViewModel
            {
                Id = created.Id,
                Title = created.Title,
                Description = created.Description,
                ImageUrl = created.ImagePath ?? "",
                UrgencyLevel = created.UrgencyLevel.ToString(),
                RequiredAmount = created.RequiredAmount.Amount,
                CollectedAmount = created.CollectedAmount.Amount,
                IsActive = created.IsActive,
                IsCompleted = created.IsCompleted,
                CreatedAt = created.CreatedOn
            };

            return Result<EmergencyCaseViewModel>.Success(
                viewModel,
                "Emergency case created successfully"
            );
        }
    }
}
