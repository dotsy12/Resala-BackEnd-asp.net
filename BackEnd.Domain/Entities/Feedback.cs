using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using System;

namespace BackEnd.Domain.Entities
{
    public sealed class Feedback : BaseEntity<int>
    {
        public int DonorId { get; private set; }
        public string Subject { get; private set; } = null!;
        public string Message { get; private set; } = null!;
        public FeedbackType Type { get; private set; }
        public FeedbackStatus Status { get; private set; }

        public Donor? Donor { get; private set; }

        private Feedback() { }

        public static Feedback Create(int donorId, string subject, string message, FeedbackType type)
        {
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException(nameof(subject));
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(nameof(message));

            return new Feedback
            {
                DonorId = donorId,
                Subject = subject.Trim(),
                Message = message.Trim(),
                Type = type,
                Status = FeedbackStatus.Pending,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };
        }

        public void ChangeStatus(FeedbackStatus status)
        {
            Status = status;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
