using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
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
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                ImageUrl = entity.ImagePath ?? "",
                UrgencyLevel = entity.UrgencyLevel.ToString(),
                RequiredAmount = entity.RequiredAmount.Amount,
                CollectedAmount = entity.CollectedAmount.Amount,
                IsActive = entity.IsActive,
                IsCompleted = entity.IsCompleted,
                CreatedAt = entity.CreatedOn
            };

            return Result<EmergencyCaseViewModel>.Success(
                viewModel,
                "Emergency case retrieved successfully"
            );
        }
    }
}
