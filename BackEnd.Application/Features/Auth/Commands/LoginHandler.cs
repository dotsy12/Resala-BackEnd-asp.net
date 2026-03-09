using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Auth.Commands;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepo;
    private readonly IDonorRepository _donorRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IJwtService _jwtService;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepo,
        IDonorRepository donorRepo,
        IStaffRepository staffRepo,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _userRepo = userRepo;
        _donorRepo = donorRepo;
        _staffRepo = staffRepo;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request, CancellationToken ct)
    {
        // 1. إيجاد المستخدم
        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user = await _userRepo.GetByPhoneAsync(request.PhoneNumber, ct);
        else if (!string.IsNullOrWhiteSpace(request.Username))
            user = await _userRepo.GetByUsernameAsync(request.Username, ct);

        if (user is null)
            return Result<LoginResponse>.Failure(
                "Invalid credentials.", ErrorType.Unauthorized);

        // 2. التحقق من الباسورد
        var passwordOk = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
            return Result<LoginResponse>.Failure(
                "Invalid credentials.", ErrorType.Unauthorized);

        // 3. التحقق من التفعيل
        if (!user.IsActive)
            return Result<LoginResponse>.Failure(
                "Account is not active. Please verify your email.",
                ErrorType.Unauthorized);

        // 4. الـ Role
        var role = await _userRepo.GetRoleAsync(user, ct) ?? "Donor";

        // 5. DonorId أو StaffId
        int? donorId = null, staffId = null;

        if (role == "Donor")
        {
            donorId = await _donorRepo.GetIdByUserIdAsync(user.Id, ct);
        }
        else
        {
            staffId = await _staffRepo.GetIdByUserIdAsync(user.Id, ct);

            // التحقق من حالة الـ Staff
            if (staffId.HasValue)
            {
                var status = await _staffRepo.GetStatusByIdAsync(staffId.Value, ct);

                if (status == AccountStatus.Locked)
                    return Result<LoginResponse>.Failure(
                        "Account is locked. Contact admin.", ErrorType.Forbidden);

                if (status == AccountStatus.Pending)
                    return Result<LoginResponse>.Failure(
                        "Account is pending approval.", ErrorType.Forbidden);
            }
        }

        // 6. توليد الـ Token
        var token = _jwtService.GenerateToken(user, role, donorId, staffId);

        return Result<LoginResponse>.Success(new LoginResponse(
            Token: token,
            Role: role,
            UserId: donorId ?? staffId ?? 0,
            Name: $"{user.FirstName} {user.LastName}".Trim(),
            PhoneNumber: user.PhoneNumber
        ), "Login successful.");
    }
}