using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.ValueObjects
{
    /// <summary>
    /// تفاصيل الدفع في الفرع — لا يحتاج عنوان لأن المتبرع يأتي للمقر
    /// </summary>
    public sealed class BranchPaymentDetails : IEquatable<BranchPaymentDetails>
    {
        public string DonorName { get; private set; } = null!;
        public string ContactNumber { get; private set; } = null!;
        public DateTime ScheduledDate { get; private set; }
        public int SlotId { get; private set; }

        private BranchPaymentDetails() { }

        public BranchPaymentDetails(
            string donorName, string contactNumber,
            DateTime scheduledDate, int slotId)
        {
            if (string.IsNullOrWhiteSpace(donorName))
                throw new ArgumentException("اسم المتبرع مطلوب.");
            if (string.IsNullOrWhiteSpace(contactNumber))
                throw new ArgumentException("رقم الاتصال مطلوب.");
            if (scheduledDate.ToUniversalTime() <= DateTime.UtcNow)
                throw new InvalidPaymentRequestException("تاريخ الموعد يجب أن يكون في المستقبل.");
            if (slotId <= 0)
                throw new ArgumentException("معرف الـ Slot غير صحيح.");

            DonorName = donorName.Trim();
            ContactNumber = contactNumber.Trim();
            ScheduledDate = scheduledDate.ToUniversalTime();
            SlotId = slotId;
        }

        public bool Equals(BranchPaymentDetails? other) =>
            other is not null && SlotId == other.SlotId && DonorName == other.DonorName;

        public override bool Equals(object? obj) => Equals(obj as BranchPaymentDetails);
        public override int GetHashCode() => HashCode.Combine(SlotId, DonorName);
    }
}