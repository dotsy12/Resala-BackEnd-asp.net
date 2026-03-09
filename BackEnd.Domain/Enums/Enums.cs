// Domain/Enums/Enums.cs
namespace BackEnd.Domain.Enums
{
    public enum UserRole { Admin = 1, Reception = 2, Donor = 3 }
    public enum StaffType { Admin = 1, Reception = 2 }
    public enum AccountStatus { Pending = 1, Active = 2, Locked = 3 }

    public enum SponsorshipStatus { Active = 1, Inactive = 2 }
    public enum UrgencyLevel { Normal = 1, Urgent = 2, Critical = 3 }

    public enum PaymentCycle
    {
        Monthly = 1,
        Quarterly = 3,
        SemiAnnual = 6
    }

    public enum SubscriptionStatus
    {
        Active = 1,
        Cancelled = 2,
        Suspended = 3,
        Expired = 4
    }

    public enum PaymentMethod
    {
        VodafoneCash = 1,
        InstaPay = 2,
        Branch = 3,
        Representative = 4
    }

    public enum PaymentStatus { Pending = 1, Verified = 2, Rejected = 3, Cancelled = 4 }
    public enum DonationType { General = 1, Emergency = 2 }

    public enum NotificationType
    {
        PaymentReminder = 1,
        LatePayment = 2,
        PaymentVerified = 3,
        Congratulations = 4,
        UrgentSponsorship = 5,
        LowFunding = 6,
        SubscriptionCreated = 7
    }
}