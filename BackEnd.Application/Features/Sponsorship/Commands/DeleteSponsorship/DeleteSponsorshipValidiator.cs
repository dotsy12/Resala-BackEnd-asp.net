using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Sponsorship.Commands.DeleteSponsorship
{
    using FluentValidation;

    namespace BackEnd.Application.Features.Sponsorship.Commands.Delete
    {
        public class DeleteSponsorshipValidator : AbstractValidator<DeleteSponsorshipCommand>
        {
            public DeleteSponsorshipValidator()
            {
                RuleFor(x => x.Id)
               .GreaterThan(0)
               .WithMessage("معرف الكفالة يجب أن يكون أكبر من 0");
            }
        }
    }
}
