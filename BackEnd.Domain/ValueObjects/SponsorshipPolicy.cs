// Domain/ValueObjects/ValueObjects.cs
namespace BackEnd.Domain.ValueObjects
{
    // ── SponsorshipPolicy ────────────────────────────────────
    public sealed class SponsorshipPolicy : IEquatable<SponsorshipPolicy>
    {
        public int GracePeriodDays { get; private set; }
        public int MaxDelayDays { get; private set; }
        public int ReminderDaysBeforeDue { get; private set; }

        public static SponsorshipPolicy Default =>
            new(gracePeriodDays: 7, maxDelayDays: 30, reminderDaysBeforeDue: 3);

        private SponsorshipPolicy() { }

        public SponsorshipPolicy(
            int gracePeriodDays, int maxDelayDays, int reminderDaysBeforeDue)
        {
            if (gracePeriodDays < 0)
                throw new ArgumentException("Grace period cannot be negative.");
            if (maxDelayDays < gracePeriodDays)
                throw new ArgumentException("MaxDelayDays must be >= GracePeriodDays.");
            if (reminderDaysBeforeDue < 1)
                throw new ArgumentException("Reminder days must be >= 1.");

            GracePeriodDays = gracePeriodDays;
            MaxDelayDays = maxDelayDays;
            ReminderDaysBeforeDue = reminderDaysBeforeDue;
        }

        public bool Equals(SponsorshipPolicy? other) =>
            other is not null &&
            GracePeriodDays == other.GracePeriodDays &&
            MaxDelayDays == other.MaxDelayDays &&
            ReminderDaysBeforeDue == other.ReminderDaysBeforeDue;

        public override bool Equals(object? obj) => Equals(obj as SponsorshipPolicy);
        public override int GetHashCode() =>
            HashCode.Combine(GracePeriodDays, MaxDelayDays, ReminderDaysBeforeDue);
    }
}