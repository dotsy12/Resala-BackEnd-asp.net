using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.ValueObjects;
using MediatR;
using BackEnd.Domain.Entities;
using BackEnd.Domain.Entities.EmergencyCase;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase
{
    public class CreateEmergencyCaseCommandHandler
      : IRequestHandler<CreateEmergencyCaseCommand, Result<EmergencyCaseViewModel>>
    {
        private readonly IEmergencyCaseRepository _repository;
        private readonly ILogger<CreateEmergencyCaseCommandHandler> _logger;

        public CreateEmergencyCaseCommandHandler(
            IEmergencyCaseRepository repository,
            ILogger<CreateEmergencyCaseCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<EmergencyCaseViewModel>> Handle(
            CreateEmergencyCaseCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء إنشاء حالة طارئة جديدة");

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

            _logger.LogInformation("تم إنشاء الحالة الطارئة بنجاح: Id={Id}", created.Id);

            return Result<EmergencyCaseViewModel>.Success(
                viewModel,
                "تم إنشاء الحالة الطارئة بنجاح."
            );
        }
    }
}