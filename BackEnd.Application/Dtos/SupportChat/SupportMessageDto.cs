using System;

namespace BackEnd.Application.Dtos.SupportChat
{
    public class SupportMessageDto
    {
        public int Id { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public bool IsRead { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public bool IsFromStaff { get; set; }
        public string ChatOwnerUserId { get; set; } = string.Empty;
    }
}
