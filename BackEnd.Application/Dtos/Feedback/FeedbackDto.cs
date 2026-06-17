using BackEnd.Domain.Enums;
using System;

namespace BackEnd.Application.Dtos.Feedback
{
    public class FeedbackDto
    {
        public int Id { get; set; }
        public int DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public FeedbackType Type { get; set; }
        public string TypeName => Type.ToString();
        public FeedbackStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedOn { get; set; }
    }
}
