using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.EmergencyCase.Commands.CreateEmergencyCase
{
    public class CreateEmergencyCaseValidator : AbstractValidator<CreateEmergencyCaseCommand>
    {
        public CreateEmergencyCaseValidator()
        {
            RuleFor(x => x.Dto.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title max length is 200");

            RuleFor(x => x.Dto.Description)
                .NotEmpty().WithMessage("Description is required");

            RuleFor(x => x.Dto.RequiredAmount)
                .GreaterThan(0).WithMessage("RequiredAmount must be greater than 0");

            RuleFor(x => x.Dto.UrgencyLevel)
                .IsInEnum().WithMessage("Invalid urgency level");
        }
    }
}