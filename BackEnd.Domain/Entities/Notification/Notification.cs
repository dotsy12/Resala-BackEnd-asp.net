using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;

namespace BackEnd.Domain.Entities.Notification
{
    public sealed class Notification : BaseEntity<int>
    {
        public int DonorId { get; private set; }
        public NotificationType Type { get; private set; }
        public string Title { get; private set; } = null!;
        public string Message { get; private set; } = null!;
        public bool IsRead { get; private set; } = false;
        public DateTime? ReadAt { get; private set; }
        public int? RelatedEntityId { get; private set; }

        public Donor? Donor { get; private set; }

        private Notification() { }

        public static Notification Create(
            int donorId, NotificationType type,
            string title, string message,
            int? relatedEntityId = null)
        {
            return new Notification
            {
                DonorId = donorId,
                Type = type,
                Title = title.Trim(),
                Message = message.Trim(),
                IsRead = false,
                RelatedEntityId = relatedEntityId,
                CreatedOn = DateTime.UtcNow
            };
        }

        public void MarkAsRead()
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}