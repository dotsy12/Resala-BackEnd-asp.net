using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Auth.Commands.Handler
{
    public class RegisterDonorByStaffHandler
        : IRequestHandler<RegisterDonorByStaffCommand, Result<RegisterDonorResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepo;
        private readonly IDonorRepository _donorRepo;
        private readonly ILogger<RegisterDonorByStaffHandler> _logger;

        public RegisterDonorByStaffHandler(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepo,
            IDonorRepository donorRepo,
            ILogger<RegisterDonorByStaffHandler> logger)
        {
            _userManager = userManager;
            _userRepo = userRepo;
            _donorRepo = donorRepo;
            _logger = logger;
        }

        public async Task<Result<RegisterDonorResponse>> Handle(
            RegisterDonorByStaffCommand request, CancellationToken ct)
        {
            _logger.LogInformation(
                "Register donor by staff started: {PhoneNumber}",
                request.PhoneNumber);

            // Check phone exists
            if (await _userRepo.PhoneExistsAsync(request.PhoneNumber, ct))
            {
                _logger.LogWarning(
                    "Register donor failed — phone already exists: {PhoneNumber}",
                    request.PhoneNumber);

                return Result<RegisterDonorResponse>.Failure(
                    "Phone number already registered.", ErrorType.Conflict);
            }

            var nameParts = request.Name.Trim().Split(' ', 2);

            var user = new ApplicationUser
            {
                UserName = request.PhoneNumber,
                Email = request.Email.ToLowerInvariant().Trim(),
                PhoneNumber = request.PhoneNumber,
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : "",
                IsActive = true,
                EmailConfirmed = true,
                CreatedOn = DateTime.UtcNow
            };

            // Create user
            var createResult = await _userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                _logger.LogError(
                    "Register donor failed — identity creation error: {PhoneNumber}, Errors: {@Errors}",
                    request.PhoneNumber,
                    createResult.Errors);

                var errors = createResult.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });

                return Result<RegisterDonorResponse>.Failure(
                    "Registration failed.", ErrorType.BadRequest, errors);
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, "Donor");

            _logger.LogInformation(
                "Role 'Donor' assigned to user: {UserId}",
                user.Id);

            // Create donor entity
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

            _logger.LogInformation(
                "Donor registered successfully by staff: {DonorId}, UserId: {UserId}",
                donor.Id, user.Id);

            return Result<RegisterDonorResponse>.Success(
                new RegisterDonorResponse(donor.Id, "Donor registered by staff."));
        }
    }
}