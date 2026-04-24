using BackEnd.Application.Common.Extensions;
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
                Image = entity.ImagePath ?? "",
                Title = entity.Title,
                Description = entity.Description,
                TargetAmount = entity.RequiredAmount.Amount,
                ReceivedAmount = entity.CollectedAmount.Amount,
                UrgencyLevel = entity.UrgencyLevel.GetDisplayName()
            }).ToList();

            return Result<IEnumerable<EmergencyCaseViewModel>>.Success(resultList);
        }
    }
 }
