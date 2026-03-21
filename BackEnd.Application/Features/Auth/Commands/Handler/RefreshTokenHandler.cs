using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IUserRepository _userRepo;
    private readonly IDonorRepository _donorRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IJwtService _jwtService;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepo,
        IUserRepository userRepo,
        IDonorRepository donorRepo,
        IStaffRepository staffRepo,
        IJwtService jwtService)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _userRepo = userRepo;
        _donorRepo = donorRepo;
        _staffRepo = staffRepo;
        _jwtService = jwtService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request, CancellationToken ct)
    {
        var storedToken = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken, ct);

        if (storedToken is null || !storedToken.IsActive)
            return Result<RefreshTokenResponse>.Failure(
                "رمز التحديث غير صحيح أو منتهي الصلاحية.", ErrorType.Unauthorized);

        // Revoke القديم
        storedToken.IsRevoked = true;
        _refreshTokenRepo.Update(storedToken);

        var user = storedToken.User;
        var role = await _userRepo.GetRoleAsync(user, ct) ?? "Donor";

        int? donorId = null, staffId = null;
        if (role == "Donor")
            donorId = await _donorRepo.GetIdByUserIdAsync(user.Id, ct);
        else
            staffId = await _staffRepo.GetIdByUserIdAsync(user.Id, ct);

        // توليد Tokens جدد
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

        return Result<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(newToken, newRefreshToken));
    }
}