using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackEnd.Application.Features.Sponsorship.Commands.Create;
using FluentValidation;

namespace BackEnd.Application.Features.Sponsorship.Commands.CreateSposorship
{
   

    public class CreateSponsorshipCommandValidator : AbstractValidator<CreateSponsorshipCommand>
    {
        public CreateSponsorshipCommandValidator()
        {
            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Name is required");

            RuleFor(x => x.Dto.Description)
                .NotEmpty().WithMessage("Description is required");

            RuleFor(x => x.Dto.TargetAmount)
                .GreaterThanOrEqualTo(0).WithMessage("TargetAmount must be >= 0");
        }
    }
}
