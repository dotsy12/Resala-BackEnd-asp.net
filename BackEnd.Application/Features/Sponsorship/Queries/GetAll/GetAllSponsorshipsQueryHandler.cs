using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.ViewModles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Sponsorship.Queries.GetAll
{


    public class GetAllSponsorshipsQueryHandler
     : IRequestHandler<GetAllSponsorshipsQuery, Result<IEnumerable<SponsorshipViewModel>>>
    {
        private readonly ISponsorshipRepository _repository;

        public GetAllSponsorshipsQueryHandler(ISponsorshipRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IEnumerable<SponsorshipViewModel>>> Handle(
            GetAllSponsorshipsQuery request,
            CancellationToken cancellationToken)
        {
            var sponsorships = await _repository.GetAllAsync(cancellationToken);

            var result = sponsorships.Select(s => new SponsorshipViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedOn
            });

            return Result<IEnumerable<SponsorshipViewModel>>.Success(result);
        }
    }
}
