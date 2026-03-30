using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Commands.DeleteEmergencyCase
{
    public class DeleteEmergencyCaseCommandHandler
        : IRequestHandler<DeleteEmergencyCaseCommand, Result<bool>>
    {
        private readonly IEmergencyCaseRepository _repository;

        public DeleteEmergencyCaseCommandHandler(IEmergencyCaseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(
            DeleteEmergencyCaseCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Get entity
            var entity = await _repository.GetByIdAsync(request.id, cancellationToken);

            if (entity == null)
            {
                return Result<bool>.Failure(
                    "Emergency case not found",
                    ErrorType.NotFound);
            }

            // 2. Business rule (optional but realistic)
            if (entity.CollectedAmount.Amount > 0)
            {
                return Result<bool>.Failure(
                    "Cannot delete a case that already has donations",
                    ErrorType.Conflict);
            }

            // 3. Soft delete (deactivate)
            entity.Deactivate();

            // 4. Save changes
            await _repository.UpdateAsync(entity, cancellationToken);

            return Result<bool>.Success(true, "Deleted successfully");
        }
    }
}
