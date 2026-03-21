using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using BackEnd.Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Sponsorship.Queries.GetById
{
   
    

    namespace BackEnd.Application.Features.Sponsorship.Queries.GetById
    {
        public class GetSponsorshipByIdQueryHandler : IRequestHandler<GetSponsorshipByIdQuery, SponsorshipViewModel>
        {
            private readonly ISponsorshipRepository _repository;

            public GetSponsorshipByIdQueryHandler(ISponsorshipRepository repository)
            {
                _repository = repository;
            }

            public async Task<SponsorshipViewModel> Handle(GetSponsorshipByIdQuery request, CancellationToken cancellationToken)
            {
                var sponsorship = await _repository.GetByIdAsync(request.id, cancellationToken);

                if (sponsorship is null)
                    throw new SponsorshipNotActiveException(request.id);

                return new SponsorshipViewModel
                {
                    Id = sponsorship.Id,
                    Name = sponsorship.Name,
                    Description = sponsorship.Description,
                    ImageUrl = sponsorship.ImagePath ?? "",
                    Icon = sponsorship.IconPath ?? "",
                    TargetAmount = sponsorship.FinancialGoal?.Amount,
                    CollectedAmount = sponsorship.TotalCollected.Amount,
                    IsActive = sponsorship.IsActive,
                    CreatedAt = sponsorship.CreatedOn
                };
            }
        }
    }
}
