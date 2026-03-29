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
                    .GreaterThan(0)
                    .WithMessage("Id must be greater than 0");

                RuleFor(x => x.Dto.Name)
                    .NotEmpty().WithMessage("Name is required")
                    .MaximumLength(200).WithMessage("Name max length is 200");

                RuleFor(x => x.Dto.Description)
                    .NotEmpty().WithMessage("Description is required");

                RuleFor(x => x.Dto.TargetAmount)
                    .GreaterThan(0)
                    .WithMessage("TargetAmount must be greater than 0")
                    .When(x => x.Dto.TargetAmount.HasValue);
            }
        }
    }
}
