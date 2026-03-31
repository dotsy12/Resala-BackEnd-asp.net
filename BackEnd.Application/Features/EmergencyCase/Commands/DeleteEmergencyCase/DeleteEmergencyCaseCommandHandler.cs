using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DeleteEmergencyCase
{
    public class DeleteEmergencyCaseCommandHandler
        : IRequestHandler<DeleteEmergencyCaseCommand, Result<bool>>
    {
        private readonly IEmergencyCaseRepository _repository;
        private readonly ILogger<DeleteEmergencyCaseCommandHandler> _logger;

        public DeleteEmergencyCaseCommandHandler(
            IEmergencyCaseRepository repository,
            ILogger<DeleteEmergencyCaseCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            DeleteEmergencyCaseCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("بدء حذف حالة طارئة: Id={Id}", request.id);

            // 1. Get entity
            var entity = await _repository.GetByIdAsync(request.id, cancellationToken);

            if (entity == null)
            {
                return Result<bool>.Failure(
                    "الحالة الطارئة غير موجودة.",
                    ErrorType.NotFound);
            }

            // 2. Business rule
            if (entity.CollectedAmount.Amount > 0)
            {
                return Result<bool>.Failure(
                    "لا يمكن حذف حالة لديها تبرعات بالفعل.",
                    ErrorType.Conflict);
            }

            // 3. Soft delete
            entity.Deactivate();

            // 4. Save
            await _repository.UpdateAsync(entity, cancellationToken);

            _logger.LogInformation("تم حذف (تعطيل) الحالة الطارئة بنجاح: Id={Id}", entity.Id);

            return Result<bool>.Success(true, "تم حذف الحالة الطارئة بنجاح.");
        }
    }
}