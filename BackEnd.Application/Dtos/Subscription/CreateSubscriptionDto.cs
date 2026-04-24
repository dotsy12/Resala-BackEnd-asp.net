using BackEnd.Domain.Enums;

namespace BackEnd.Application.Dtos.Subscription
{
    /// <summary>بيانات إنشاء الاشتراك — بدون بيانات الدفع</summary>
    public class CreateSubscriptionDto
    {
        public int SponsorshipId { get; set; }
        public decimal Amount { get; set; }
        public PaymentCycle PaymentCycle { get; set; }
    }
}