using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Auth.Commands.Handler
{
    public class CreateStaffHandler
        : IRequestHandler<CreateStaffCommand, Result<CreateStaffResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepo;
        private readonly IStaffRepository _staffRepo;
        private readonly ILogger<CreateStaffHandler> _logger;

        public CreateStaffHandler(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepo,
            IStaffRepository staffRepo,
            ILogger<CreateStaffHandler> logger)
        {
            _userManager = userManager;
            _userRepo = userRepo;
            _staffRepo = staffRepo;
            _logger = logger;
        }

        public async Task<Result<CreateStaffResponse>> Handle(
            CreateStaffCommand request, CancellationToken ct)
        {
            _logger.LogInformation(
                "Create staff request started: {Username}, Type: {StaffType}",
                request.Username, request.StaffType);

            // Check username exists
            var usernameExists = await _userRepo.GetByUsernameAsync(request.Username, ct);
            if (usernameExists is not null)
            {
                _logger.LogWarning(
                    "Create staff failed — username already exists: {Username}",
                    request.Username);

                return Result<CreateStaffResponse>.Failure(
                    "اسم المستخدم مأخوذ مسبقاً.", ErrorType.Conflict);
            }

            var nameParts = request.Name.Trim().Split(' ', 2);

            var user = new ApplicationUser
            {
                UserName = request.Username,
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
                    "Create staff failed — identity creation error: {Username}, Errors: {@Errors}",
                    request.Username,
                    createResult.Errors);

                var errors = createResult.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });

                return Result<CreateStaffResponse>.Failure(
                    "فشل إنشاء حساب الموظف.", ErrorType.BadRequest, errors);
            }

            // Assign role
            var roleName = request.StaffType == StaffType.Admin ? "Admin" : "Reception";
            await _userManager.AddToRoleAsync(user, roleName);

            _logger.LogInformation(
                "Role assigned to staff: {UserId}, Role: {Role}",
                user.Id, roleName);

            // Create staff entity
            var staff = StaffMember.Create(
                userId: user.Id,
                firstName: nameParts[0],
                lastName: nameParts.Length > 1 ? nameParts[1] : "",
                username: request.Username,
                email: request.Email,
                phone: request.PhoneNumber,
                staffType: request.StaffType
            );

            await _staffRepo.AddAsync(staff, ct);
            await _staffRepo.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Staff created successfully: {StaffId}, Username: {Username}, Type: {StaffType}",
                staff.Id, staff.Username, request.StaffType);

            return Result<CreateStaffResponse>.Success(
                new CreateStaffResponse(staff.Id, staff.Username));
        }
    }
}