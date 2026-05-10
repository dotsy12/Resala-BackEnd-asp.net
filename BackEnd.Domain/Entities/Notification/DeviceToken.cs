using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;

namespace BackEnd.Domain.Entities.Notification
{
    public sealed class DeviceToken : BaseEntity<int>
    {
        public int DonorId { get; private set; }
        public string Token { get; private set; } = null!;
        public string? DeviceType { get; private set; } // e.g., "Android", "iOS"
        public DateTime LastUsed { get; private set; }

        public Donor? Donor { get; private set; }

        private DeviceToken() { }

        public static DeviceToken Create(int donorId, string token, string? deviceType = null)
        {
            return new DeviceToken
            {
                DonorId = donorId,
                Token = token.Trim(),
                DeviceType = deviceType,
                LastUsed = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow
            };
        }

        public void UpdateLastUsed()
        {
            LastUsed = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
