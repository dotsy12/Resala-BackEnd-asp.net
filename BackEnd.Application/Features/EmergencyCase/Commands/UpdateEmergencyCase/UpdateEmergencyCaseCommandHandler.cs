using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.EmergencyCase.Commands.UpdateEmergencyCase
{
    public class UpdateEmergencyCaseCommandHandler
      : IRequestHandler<UpdateEmergencyCaseCommand, Result<EmergencyCaseViewModel>>
    {
        private readonly IEmergencyCaseRepository _repository;
        private readonly ILogger<UpdateEmergencyCaseCommandHandler> _logger;

        public UpdateEmergencyCaseCommandHandler(
            IEmergencyCaseRepository repository,
            ILogger<UpdateEmergencyCaseCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<EmergencyCaseViewModel>> Handle(
            UpdateEmergencyCaseCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء تعديل حالة طارئة: Id={Id}", request.Id);

            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null)
            {
                return Result<EmergencyCaseViewModel>.Failure(
                    "الحالة الطارئة غير موجودة.",
                    ErrorType.NotFound);
            }

            var dto = request.Dto;

            // ✅ Update basic info
            entity.UpdateDetails(dto.Title, dto.Description, dto.ImageUrl);

            // ✅ Update urgency
            entity.SetUrgency(dto.UrgencyLevel);

            // ✅ Update RequiredAmount
            if (dto.RequiredAmount.HasValue)
            {
                if (dto.RequiredAmount.Value <= 0)
                {
                    return Result<EmergencyCaseViewModel>.Failure(
                        "المبلغ المطلوب يجب أن يكون أكبر من صفر.",
                        ErrorType.BadRequest);
                }

                var money = new Money(dto.RequiredAmount.Value, "EGP");
                entity.UpdateRequiredAmount(money);
            }

            // ✅ Activate / Deactivate
            if (dto.IsActive)
                entity.Activate();
            else
                entity.Deactivate();

            // ✅ Save
            await _repository.UpdateAsync(entity, cancellationToken);

            // ✅ Mapping
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

            _logger.LogInformation("تم تعديل الحالة الطارئة بنجاح: Id={Id}", entity.Id);

            return Result<EmergencyCaseViewModel>.Success(
                vm,
                "تم تعديل الحالة الطارئة بنجاح."
            );
        }
    }
}