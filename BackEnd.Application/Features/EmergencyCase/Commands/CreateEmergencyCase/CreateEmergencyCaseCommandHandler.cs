using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Entities;
using BackEnd.Domain.Entities.EmergencyCase;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.ValueObjects;
using MediatR;
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
            try
            {
                _logger.LogInformation("بدء إنشاء حالة طارئة جديدة");

                // Value Object
                var requiredAmount = new Money(request.RequiredAmount);

                // Domain Entity
                var entity = BackEnd.Domain.Entities.EmergencyCase.EmergencyCase.Create(
                    request.Title,
                    request.Description,
                    request.UrgencyLevel,
                    requiredAmount,
                    request.ImageUrl
                );

                var created = await _repository.CreateAsync(entity, cancellationToken);

                _logger.LogInformation("تم إنشاء الحالة الطارئة بنجاح. Id={Id}", created.Id);

                // Mapping
                var viewModel = new EmergencyCaseViewModel
                {
                    Image = created.ImagePath ?? "",
                    Title = created.Title,
                    Description = created.Description,
                    TargetAmount = created.RequiredAmount.Amount,
                    ReceivedAmount = created.CollectedAmount.Amount,
                    CriticalPriority = created.UrgencyLevel == BackEnd.Domain.Enums.UrgencyLevel.Critical
                };

                return Result<EmergencyCaseViewModel>.Success(
                    viewModel,
                    "تم إنشاء الحالة الطارئة بنجاح."
                );
            }
            catch (DomainException)
            {
                // سيُعالج في الـ Global Exception Handling Middleware
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ غير متوقع أثناء إنشاء حالة طارئة");

                return Result<EmergencyCaseViewModel>.Failure(
                    "حدث خطأ غير متوقع أثناء إنشاء الحالة الطارئة",
                    ErrorType.ServerError
                );
            }
        }

    }
}