using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using BackEnd.Application.Features.InKindDonation.Commands.CreateInKindDonation;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.InKindDonation.Commands.CreateInKindDonation
{
    public class RecordInKindDonationHandler
        : IRequestHandler<RecordInKindDonationCommand, Result<InKindDonationDto>>
    {
        private readonly IInKindDonationRepository _repo;
        private readonly IDonorRepository _donorRepo;
        private readonly IStaffRepository _staffRepo;
        private readonly ILogger<RecordInKindDonationHandler> _logger;

        public RecordInKindDonationHandler(
            IInKindDonationRepository repo,
            IDonorRepository donorRepo,
            IStaffRepository staffRepo,
            ILogger<RecordInKindDonationHandler> logger)
        {
            _repo = repo;
            _donorRepo = donorRepo;
            _staffRepo = staffRepo;
            _logger = logger;
        }

        public async Task<Result<InKindDonationDto>> Handle(
            RecordInKindDonationCommand request, CancellationToken ct)
        {
            _logger.LogInformation(
                "تسجيل تبرع عيني — DonorId: {DonorId} بواسطة StaffId: {StaffId}",
                request.DonorId, request.RecordedByStaffId);

            // ✅ التحقق من وجود Donor عن طريق الـ ID مباشرة
            var donorExists = await _donorRepo.GetByIdAsync(request.DonorId, ct);
            if (donorExists is null)
                return Result<InKindDonationDto>.Failure(
                    "المتبرع غير موجود.", ErrorType.NotFound);

            // ✅ التحقق من وجود Staff عن طريق الـ ID
            var staffExists = await _staffRepo.GetByIdAsync(request.RecordedByStaffId, ct);
            if (staffExists is null)
                return Result<InKindDonationDto>.Failure(
                    "الموظف غير موجود.", ErrorType.NotFound);

            var donation =BackEnd.Domain.Entities.Payment.InKindDonation.Create(
                donorId: request.DonorId,
                donationTypeName: request.DonationTypeName,
                quantity: request.Quantity,
                description: request.Description,
                recordedByStaffId: request.RecordedByStaffId
            );

            await _repo.AddAsync(donation, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("تم تسجيل التبرع العيني: Id={Id}", donation.Id);

            return Result<InKindDonationDto>.Success(new InKindDonationDto(
                Id: donation.Id,
                DonorId: donorExists.Id,
                DonorName: $"{donorExists.FullName.FirstName} {donorExists.FullName.LastName}".Trim(),
                DonationTypeName: donation.DonationTypeName,
                Quantity: donation.Quantity,
                Description: donation.Description,
                RecordedByStaffId: staffExists.Id,
                RecordedByStaffName: $"{staffExists.FullName.FirstName} {staffExists.FullName.LastName}".Trim(),
                RecordedAt: donation.RecordedAt,
                CreatedOn: donation.CreatedOn
            ), "تم تسجيل التبرع العيني بنجاح.");
        }
    }
}