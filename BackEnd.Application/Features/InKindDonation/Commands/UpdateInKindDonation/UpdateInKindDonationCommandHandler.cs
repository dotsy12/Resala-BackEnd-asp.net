using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using BackEnd.Application.Features.InKindDonation.Commands.UpdateInKindDonation;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateInKindDonationHandler
    : IRequestHandler<UpdateInKindDonationCommand, Result<InKindDonationDto>>
{
    private readonly IInKindDonationRepository _repo;
    private readonly ILogger<UpdateInKindDonationHandler> _logger;

    public UpdateInKindDonationHandler(
        IInKindDonationRepository repo,
        ILogger<UpdateInKindDonationHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Result<InKindDonationDto>> Handle(
        UpdateInKindDonationCommand request, CancellationToken ct)
    {
        var donation = await _repo.GetByIdAsync(request.Id, ct);
        if (donation is null)
            return Result<InKindDonationDto>.Failure(
                "التبرع العيني غير موجود.", ErrorType.NotFound);

        donation.Update(
            request.DonationTypeName,
            request.Quantity,
            request.Description);

        _repo.Update(donation);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("تم تعديل التبرع العيني: Id={Id}", donation.Id);

        return Result<InKindDonationDto>.Success(new InKindDonationDto(
            Id: donation.Id,
            DonorId: donation.DonorId,
            DonorName: donation.Donor is not null
                ? $"{donation.Donor.FullName.FirstName} {donation.Donor.FullName.LastName}".Trim()
                : "",
            DonationTypeName: donation.DonationTypeName,
            Quantity: donation.Quantity,
            Description: donation.Description,
            RecordedByStaffId: donation.RecordedByStaffId,
            RecordedByStaffName: donation.RecordedBy is not null
                ? $"{donation.RecordedBy.FullName.FirstName} {donation.RecordedBy.FullName.LastName}".Trim()
                : "",
            RecordedAt: donation.RecordedAt,
            CreatedOn: donation.CreatedOn
        ), "تم تعديل التبرع العيني بنجاح.");
    }
}