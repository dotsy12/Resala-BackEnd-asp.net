using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Events;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Sponsorship
{
    public sealed class SponsorshipSubscription : BaseEntity<int>
    {
        public int DonorId { get; private set; }
        public int SponsorshipId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public PaymentCycle PaymentCycle { get; private set; }
        public SubscriptionStatus Status { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime NextPaymentDate { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public string? CancelReason { get; private set; }

        public Donor? Donor { get; private set; }
        public Sponsorship? Sponsorship { get; private set; }

        private SponsorshipSubscription() { }

        public static SponsorshipSubscription Create(
            int donorId, int sponsorshipId,
            Sponsorship sponsorship,
            Money amount, PaymentCycle cycle)
        {
            if (!sponsorship.IsActive)
                throw new SponsorshipNotActiveException(sponsorshipId);
            if (amount.Amount <= 0)
                throw new InvalidMoneyAmountException(amount.Amount);

            var startDate = DateTime.UtcNow;
            var sub = new SponsorshipSubscription
            {
                DonorId = donorId,
                SponsorshipId = sponsorshipId,
                Amount = amount,
                PaymentCycle = cycle,
                Status = SubscriptionStatus.Active,
                StartDate = startDate,
                NextPaymentDate = startDate.AddMonths((int)cycle),
                CreatedOn = startDate
            };

            sub.AddDomainEvent(new SubscriptionCreatedEvent(0, donorId, sponsorshipId));
            return sub;
        }

        /// <summary>تحديث قيمة الاشتراك</summary>
        public void UpdateAmount(Money newAmount)
        {
            if (Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired)
                throw new InvalidSubscriptionOperationException(
                    "لا يمكن تعديل اشتراك ملغي أو منتهي.");
            if (newAmount.Amount <= 0)
                throw new InvalidMoneyAmountException(newAmount.Amount);

            Amount = newAmount;
            UpdatedOn = DateTime.UtcNow;
        }

        /// <summary>تحديث دورة الدفع</summary>
        public void UpdatePaymentCycle(PaymentCycle newCycle)
        {
            if (Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired)
                throw new InvalidSubscriptionOperationException(
                    "لا يمكن تعديل اشتراك ملغي أو منتهي.");

            PaymentCycle = newCycle;
            // إعادة حساب الدفعة القادمة من تاريخ اليوم
            NextPaymentDate = DateTime.UtcNow.AddMonths((int)newCycle);
            UpdatedOn = DateTime.UtcNow;
        }

        /// <summary>إلغاء الاشتراك</summary>
        public void Cancel(string? reason = null)
        {
            if (Status == SubscriptionStatus.Cancelled)
                throw new InvalidSubscriptionOperationException("الاشتراك ملغي بالفعل.");

            Status = SubscriptionStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancelReason = reason?.Trim();
            UpdatedOn = DateTime.UtcNow;

            AddDomainEvent(new SubscriptionCancelledEvent(Id, DonorId, reason));
        }

        /// <summary>تعليق الاشتراك (بسبب تأخر الدفع)</summary>
        public void Suspend()
        {
            if (Status != SubscriptionStatus.Active)
                throw new InvalidSubscriptionOperationException(
                    "يمكن تعليق الاشتراكات النشطة فقط.");

            Status = SubscriptionStatus.Suspended;
            UpdatedOn = DateTime.UtcNow;
            AddDomainEvent(new LatePaymentDetectedEvent(Id, DonorId, NextPaymentDate));
        }

        /// <summary>تقديم تاريخ الدفعة القادمة بعد التأكيد</summary>
        public void AdvancePaymentDate()
        {
            NextPaymentDate = NextPaymentDate.AddMonths((int)PaymentCycle);
            if (Status == SubscriptionStatus.Suspended)
                Status = SubscriptionStatus.Active;
            UpdatedOn = DateTime.UtcNow;
        }

        public bool IsLate(int gracePeriodDays) =>
            Status == SubscriptionStatus.Active &&
            DateTime.UtcNow > NextPaymentDate.AddDays(gracePeriodDays);
    }
}