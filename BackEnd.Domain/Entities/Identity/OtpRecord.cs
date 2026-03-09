using BackEnd.Domain.Common;

namespace BackEnd.Domain.Entities.Identity
{
    public sealed class OtpRecord : BaseEntity<int>
    {
        public string Email { get; private set; } = null!;
        public string Code { get; private set; } = null!;
        public string Purpose { get; private set; } = null!; // "EmailVerification" | "PasswordReset"
        public DateTime ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; } = false;

        private OtpRecord() { }

        public static OtpRecord Create(string email, string code, string purpose, int expiryMinutes = 10)
        {
            return new OtpRecord
            {
                Email = email.ToLowerInvariant().Trim(),
                Code = code,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                IsUsed = false,
                CreatedOn = DateTime.UtcNow
            };
        }

        public bool IsValid() => !IsUsed && DateTime.UtcNow <= ExpiresAt;

        public void MarkAsUsed()
        {
            IsUsed = true;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}