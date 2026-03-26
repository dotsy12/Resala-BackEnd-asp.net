using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IUserRepository _userRepo;
    private readonly IDonorRepository _donorRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IJwtService _jwtService;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepo,
        IUserRepository userRepo,
        IDonorRepository donorRepo,
        IStaffRepository staffRepo,
        IJwtService jwtService,
        ILogger<RefreshTokenHandler> logger)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _userRepo = userRepo;
        _donorRepo = donorRepo;
        _staffRepo = staffRepo;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Refresh token request started");

        var storedToken = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken, ct);

        if (storedToken is null)
        {
            _logger.LogWarning("Refresh token failed — token not found");
            return Result<RefreshTokenResponse>.Failure(
                "رمز التحديث غير صحيح أو منتهي الصلاحية.", ErrorType.Unauthorized);
        }

        if (!storedToken.IsActive)
        {
            _logger.LogWarning("Refresh token failed — token is inactive or expired: {UserId}", storedToken.UserId);
            return Result<RefreshTokenResponse>.Failure(
                "رمز التحديث غير صحيح أو منتهي الصلاحية.", ErrorType.Unauthorized);
        }

        // Revoke old token
        storedToken.IsRevoked = true;
        _refreshTokenRepo.Update(storedToken);

        _logger.LogInformation(
            "Old refresh token revoked: {UserId}", storedToken.UserId);

        var user = storedToken.User;

        var role = await _userRepo.GetRoleAsync(user, ct) ?? "Donor";

        int? donorId = null, staffId = null;

        if (role == "Donor")
        {
            donorId = await _donorRepo.GetIdByUserIdAsync(user.Id, ct);
        }
        else
        {
            staffId = await _staffRepo.GetIdByUserIdAsync(user.Id, ct);
        }

        _logger.LogInformation(
            "User data resolved for refresh token: {UserId}, Role: {Role}",
            user.Id, role);

        // Generate new tokens
        var newToken = _jwtService.GenerateToken(user, role, donorId, staffId);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepo.AddAsync(newRefreshTokenEntity, ct);
        await _refreshTokenRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "New refresh token generated successfully: {UserId}", user.Id);

        return Result<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(newToken, newRefreshToken));
    }
}