using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _db;
        public OtpService(ApplicationDbContext db) => _db = db;

        public string GenerateOtp()
        {
            // OTP آمن 6 أرقام
            var bytes = new byte[4];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1_000_000;
            return number.ToString("D6");
        }

        public async Task SaveOtpAsync(string email, string code, string purpose,
            CancellationToken ct = default)
        {
            // إلغاء أي OTP قديم لنفس الغرض
            var oldOtps = await _db.OtpRecords
                .Where(o => o.Email == email.ToLower() &&
                            o.Purpose == purpose &&
                            !o.IsUsed)
                .ToListAsync(ct);

            foreach (var old in oldOtps)
                old.MarkAsUsed();

            var otp = OtpRecord.Create(email, code, purpose, expiryMinutes: 10);
            _db.OtpRecords.Add(otp);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> ValidateOtpAsync(string email, string code, string purpose,
            CancellationToken ct = default)
        {
            var otp = await _db.OtpRecords
                .Where(o => o.Email == email.ToLower() &&
                            o.Code == code &&
                            o.Purpose == purpose &&
                            !o.IsUsed)
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefaultAsync(ct);

            if (otp is null || !otp.IsValid())
                return false;

            otp.MarkAsUsed();
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}