namespace BackEnd.Application.Interfaces.Services
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task SaveOtpAsync(string email, string code, string purpose,
            CancellationToken ct = default);
        Task<bool> ValidateOtpAsync(string email, string code, string purpose,
            CancellationToken ct = default);
    }
}