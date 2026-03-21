using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands.Handler
{
    public class RegisterDonorByStaffHandler
        : IRequestHandler<RegisterDonorByStaffCommand, Result<RegisterDonorResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepo;
        private readonly IDonorRepository _donorRepo;

        public RegisterDonorByStaffHandler(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepo,
            IDonorRepository donorRepo)
        {
            _userManager = userManager;
            _userRepo = userRepo;
            _donorRepo = donorRepo;
        }

        public async Task<Result<RegisterDonorResponse>> Handle(
            RegisterDonorByStaffCommand request, CancellationToken ct)
        {
            if (await _userRepo.PhoneExistsAsync(request.PhoneNumber, ct))
                return Result<RegisterDonorResponse>.Failure(
                    "Phone number already registered.", ErrorType.Conflict);

            var nameParts = request.Name.Trim().Split(' ', 2);
            var user = new ApplicationUser
            {
                UserName = request.PhoneNumber,
                Email = request.Email.ToLowerInvariant().Trim(),
                PhoneNumber = request.PhoneNumber,
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : "",
                IsActive = true,          // الـ Staff يفعّله مباشرة
                EmailConfirmed = true,
                CreatedOn = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });
                return Result<RegisterDonorResponse>.Failure(
                    "Registration failed.", ErrorType.BadRequest, errors);
            }

            await _userManager.AddToRoleAsync(user, "Donor");

            var donor = Donor.Create(
                userId: user.Id,
                firstName: nameParts[0],
                lastName: nameParts.Length > 1 ? nameParts[1] : "",
                email: request.Email,
                phoneNumber: request.PhoneNumber,
                job: request.Job,
                landline: request.Landline
            );

            await _donorRepo.AddAsync(donor, ct);
            await _donorRepo.SaveChangesAsync(ct);

            return Result<RegisterDonorResponse>.Success(
                new RegisterDonorResponse(donor.Id, "Donor registered by staff."));
        }
    }
}