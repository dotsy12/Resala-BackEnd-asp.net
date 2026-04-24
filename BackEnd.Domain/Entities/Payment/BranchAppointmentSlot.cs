using BackEnd.Domain.Common;

namespace BackEnd.Domain.Entities.Payment
{
    /// <summary>
    /// موعد متاح في مقر الجمعية — يُنشئه الأدمن فقط
    /// المتبرع يختار منه
    /// </summary>
    public sealed class BranchAppointmentSlot : BaseEntity<int>
    {
        public DateTime SlotDate { get; private set; }
        public TimeSpan OpenFrom { get; private set; }
        public TimeSpan OpenTo { get; private set; }
        public int MaxCapacity { get; private set; }
        public int BookedCount { get; private set; }
        public bool IsActive { get; private set; }
        public string? Notes { get; private set; }

        private BranchAppointmentSlot() { }

        public static BranchAppointmentSlot Create(
            DateTime slotDate, TimeSpan openFrom, TimeSpan openTo,
            int maxCapacity, string? notes = null)
        {
            if (slotDate.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("تاريخ الـ Slot يجب أن يكون اليوم أو في المستقبل.");

            if (openFrom >= openTo)
                throw new ArgumentException("وقت البداية يجب أن يكون قبل وقت النهاية.");

            if (maxCapacity <= 0)
                throw new ArgumentException("السعة القصوى يجب أن تكون أكبر من صفر.");

            return new BranchAppointmentSlot
            {
                SlotDate = slotDate.Date.ToUniversalTime(),
                OpenFrom = openFrom,
                OpenTo = openTo,
                MaxCapacity = maxCapacity,
                BookedCount = 0,
                IsActive = true,
                Notes = notes?.Trim(),
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <summary>هل الـ Slot متاح للحجز</summary>
        public bool HasAvailableCapacity =>
            IsActive && SlotDate.Date >= DateTime.UtcNow.Date && BookedCount < MaxCapacity;

        public int AvailableSpots => MaxCapacity - BookedCount;

        /// <summary>حجز مكان</summary>
        public void Book()
        {
            if (!HasAvailableCapacity)
                throw new InvalidOperationException("هذا الموعد ممتلئ أو غير متاح.");

            BookedCount++;
            UpdatedOn = DateTime.UtcNow;
        }

        /// <summary>إلغاء حجز — يُفرج عن مكان</summary>
        public void Unbook()
        {
            if (BookedCount > 0)
            {
                BookedCount--;
                UpdatedOn = DateTime.UtcNow;
            }
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedOn = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedOn = DateTime.UtcNow;
        }

        public void UpdateCapacity(int newCapacity)
        {
            if (newCapacity < BookedCount)
                throw new InvalidOperationException(
                    $"السعة الجديدة ({newCapacity}) أقل من عدد الحجوزات الحالية ({BookedCount}).");

            MaxCapacity = newCapacity;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}