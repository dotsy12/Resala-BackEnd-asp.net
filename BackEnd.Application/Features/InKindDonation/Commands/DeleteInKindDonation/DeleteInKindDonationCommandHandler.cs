using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.InKindDonation.Commands.DeleteInKindDonation
{
    public class DeleteInKindDonationHandler
      : IRequestHandler<DeleteInKindDonationCommand, Result<string>>
    {
        private readonly IInKindDonationRepository _repo;
        private readonly ILogger<DeleteInKindDonationHandler> _logger;

        public DeleteInKindDonationHandler(
            IInKindDonationRepository repo,
            ILogger<DeleteInKindDonationHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(
            DeleteInKindDonationCommand request, CancellationToken ct)
        {
            var donation = await _repo.GetByIdAsync(request.Id, ct);
            if (donation is null)
                return Result<string>.Failure(
                    "التبرع العيني غير موجود.", ErrorType.NotFound);

            _repo.Remove(donation);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("تم حذف التبرع العيني: Id={Id}", request.Id);

            return Result<string>.Success("تم حذف التبرع العيني بنجاح.");
        }
    }
}
