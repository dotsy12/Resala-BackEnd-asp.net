using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace BackEnd.Application.Features.Sponsorship.Commands.UpdateSponsorship
{


    namespace BackEnd.Application.Features.Sponsorship.Commands.Update
    {
        public class UpdateSponsorshipValidator : AbstractValidator<UpdateSponsorshipCommand>
        {
            public UpdateSponsorshipValidator()
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0);

                RuleFor(x => x.Dto.Name)
                    .NotEmpty()
                    .MaximumLength(200);

                RuleFor(x => x.Dto.Description)
                    .NotEmpty();

                RuleFor(x => x.Dto.TargetAmount)
                    .GreaterThan(0)
                    .When(x => x.Dto.TargetAmount.HasValue);
            }
        }
    }
}
