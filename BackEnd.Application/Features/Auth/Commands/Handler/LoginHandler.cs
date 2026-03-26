using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Auth.Commands;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepo;
    private readonly IDonorRepository _donorRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepo,
        IDonorRepository donorRepo,
        IStaffRepository staffRepo,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepo,
         ILogger<LoginHandler> logger)
    {
        _userManager = userManager;
        _userRepo = userRepo;
        _donorRepo = donorRepo;
        _staffRepo = staffRepo;
        _jwtService = jwtService;
        _refreshTokenRepo = refreshTokenRepo;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request, CancellationToken ct)
    {
        // 1. Find user
        var identifier = request.PhoneNumber ?? request.Username;
        _logger.LogInformation("Login attempt: {Identifier}", identifier);

        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user = await _userRepo.GetByPhoneAsync(request.PhoneNumber, ct);
        else if (!string.IsNullOrWhiteSpace(request.Username))
            user = await _userRepo.GetByUsernameAsync(request.Username, ct);

        if (user is null)
        {
            _logger.LogWarning("Login failed — user not found: {Identifier}", identifier);
            return Result<LoginResponse>.Failure(
                    "بيانات الدخول غير صحيحة.", ErrorType.Unauthorized);
        }

        // 2. Check password
        var passwordOk = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
        {
            _logger.LogWarning("Login failed — invalid password: {Identifier}", identifier);

            return Result<LoginResponse>.Failure(
                    "بيانات الدخول غير صحيحة.", ErrorType.Unauthorized);
        }

        // 3. Check activation
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed — inactive account: {UserId}", user.Id);
            return Result<LoginResponse>.Failure(
                "الحساب غير مفعّل. يرجى التحقق من بريدك الإلكتروني.",
                ErrorType.Unauthorized);
        }

        // 4. Get role
        var role = await _userRepo.GetRoleAsync(user, ct) ?? "Donor";

        // 5. DonorId or StaffId
        int? donorId = null, staffId = null;

        if (role == "Donor")
        {
            donorId = await _donorRepo.GetIdByUserIdAsync(user.Id, ct);
        }
        else
        {
            staffId = await _staffRepo.GetIdByUserIdAsync(user.Id, ct);

            // Check staff status
            if (staffId.HasValue)
            {
                var status = await _staffRepo.GetStatusByIdAsync(staffId.Value, ct);

                if (status == AccountStatus.Locked)
                    return Result<LoginResponse>.Failure(
                        "الحساب موقوف. تواصل مع الأدمن.", ErrorType.Forbidden);

                if (status == AccountStatus.Pending)
                    return Result<LoginResponse>.Failure(
                      "الحساب قيد المراجعة.", ErrorType.Forbidden);
            }
        }

        // 6. Generate token
        var token = _jwtService.GenerateToken(user, role, donorId, staffId);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepo.AddAsync(refreshTokenEntity, ct);
        await _refreshTokenRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
          "Login successful: {UserId} — Role: {Role}", user.Id, role);

        return Result<LoginResponse>.Success(new LoginResponse(
            Token: token,
            RefreshToken: refreshToken,
            Role: role,
            UserId: donorId ?? staffId ?? 0,
            Name: $"{user.FirstName} {user.LastName}".Trim(),
            PhoneNumber: user.PhoneNumber
        ), "تم تسجيل الدخول بنجاح.");
    }
}