using BackEnd.Domain.Common;
using System;

namespace BackEnd.Domain.Entities
{
    public sealed class SupportMessage : BaseEntity<int>
    {
        public string MessageText { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public bool IsFromStaff { get; set; }
        public string ChatOwnerUserId { get; set; } = string.Empty;

        // Private constructor for EF Core
        private SupportMessage() { }

        public static SupportMessage Create(
            string messageText,
            string senderId,
            string senderName,
            bool isFromStaff,
            string chatOwnerUserId)
        {
            return new SupportMessage
            {
                MessageText = messageText,
                SenderId = senderId,
                SenderName = senderName,
                IsFromStaff = isFromStaff,
                ChatOwnerUserId = chatOwnerUserId,
                IsRead = false,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };
        }
    }
}
