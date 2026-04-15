using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Queries.GetEmergencyCaseById
{
    public class GetEmergencyCaseByIdQueryHandler
        : IRequestHandler<GetEmergencyCaseByIdQuery, Result<EmergencyCaseViewModel>>
    {
        private readonly IEmergencyCaseRepository _repository;

        public GetEmergencyCaseByIdQueryHandler(IEmergencyCaseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<EmergencyCaseViewModel>> Handle(
            GetEmergencyCaseByIdQuery request,
            CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null)
            {
                return Result<EmergencyCaseViewModel>.Failure(
                    "Emergency case not found",
                    ErrorType.NotFound
                );
            }

            var viewModel = new EmergencyCaseViewModel
            {
                Image = entity.ImagePath ?? "",
                Title = entity.Title,
                Description = entity.Description,
                TargetAmount = entity.RequiredAmount.Amount,
                ReceivedAmount = entity.CollectedAmount.Amount,
                CriticalPriority = entity.UrgencyLevel == UrgencyLevel.Critical
            };

            return Result<EmergencyCaseViewModel>.Success(
                viewModel,
                "Emergency case retrieved successfully"
            );
        }
    }
}
