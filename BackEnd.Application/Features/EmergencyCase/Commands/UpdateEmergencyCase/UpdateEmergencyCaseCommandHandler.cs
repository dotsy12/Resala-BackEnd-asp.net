using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Enums;
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

            // ✅ Update basic info
            entity.UpdateDetails(request.Title, request.Description, request.ImageUrl);

            // ✅ Update urgency
            entity.SetUrgency(request.UrgencyLevel);

            // ✅ Update RequiredAmount
            if (request.RequiredAmount.HasValue)
            {
                if (request.RequiredAmount.Value <= 0)
                {
                    return Result<EmergencyCaseViewModel>.Failure(
                        "المبلغ المطلوب يجب أن يكون أكبر من صفر.",
                        ErrorType.BadRequest);
                }

                var money = new Money(request.RequiredAmount.Value, "EGP");
                entity.UpdateRequiredAmount(money);
            }

            // ✅ Activate / Deactivate
            if (request.IsActive)
                entity.Activate();
            else
                entity.Deactivate();

            // ✅ Save
            await _repository.UpdateAsync(entity, cancellationToken);

            // ✅ Mapping
            var vm = new EmergencyCaseViewModel
            {
                Image = entity.ImagePath ?? "",
                Title = entity.Title,
                Description = entity.Description,
                TargetAmount = entity.RequiredAmount.Amount,
                ReceivedAmount = entity.CollectedAmount.Amount,
                CriticalPriority = entity.UrgencyLevel == UrgencyLevel.Critical
            };

            _logger.LogInformation("تم تعديل الحالة الطارئة بنجاح: Id={Id}", entity.Id);

            return Result<EmergencyCaseViewModel>.Success(
                vm,
                "تم تعديل الحالة الطارئة بنجاح."
            );
        }
    }
}