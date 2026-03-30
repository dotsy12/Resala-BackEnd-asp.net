using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Queries.GetAllEmergenciesCasies
{
    public class GetAllEmergencyCasesQueryHandler
     : IRequestHandler<GetAllEmergencyCasesQuery, Result<IEnumerable<EmergencyCaseViewModel>>>
    {
        private readonly IEmergencyCaseRepository _repository;

        public GetAllEmergencyCasesQueryHandler(IEmergencyCaseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IEnumerable<EmergencyCaseViewModel>>> Handle(
            GetAllEmergencyCasesQuery request,
            CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync(cancellationToken);

            var resultList = entities.Select(entity => new EmergencyCaseViewModel
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
            });

            return Result<IEnumerable<EmergencyCaseViewModel>>.Success(resultList);
        }
    }
 }
