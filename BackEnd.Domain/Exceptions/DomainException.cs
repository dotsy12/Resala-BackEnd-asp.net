// Domain/Exceptions/DomainException.cs
namespace BackEnd.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string Code { get; }
        protected DomainException(string message, string code)
            : base(message) => Code = code;
    }

    public sealed class InvalidMoneyAmountException : DomainException
    {
        public InvalidMoneyAmountException(decimal amount)
            : base($"Amount '{amount}' must be >= 0.", "INVALID_AMOUNT") { }
    }

    public sealed class InvalidEmailException : DomainException
    {
        public InvalidEmailException(string email)
            : base($"'{email}' is not a valid email.", "INVALID_EMAIL") { }
    }

    public sealed class InvalidPhoneNumberException : DomainException
    {
        public InvalidPhoneNumberException(string phone)
            : base($"'{phone}' is not a valid Egyptian phone number.", "INVALID_PHONE") { }
    }

    public sealed class SponsorshipNotActiveException : DomainException
    {
        public SponsorshipNotActiveException(int id)
            : base($"Sponsorship '{id}' is not active.", "SPONSORSHIP_INACTIVE") { }
    }

    public sealed class DuplicateSubscriptionException : DomainException
    {
        public DuplicateSubscriptionException(int donorId, int sponsorshipId)
            : base($"Donor '{donorId}' already subscribed to sponsorship '{sponsorshipId}'.",
                   "DUPLICATE_SUBSCRIPTION")
        { }
    }

    public sealed class InvalidPaymentRequestException : DomainException
    {
        public InvalidPaymentRequestException(string reason)
            : base($"Invalid payment request: {reason}", "INVALID_PAYMENT") { }
    }

    public sealed class InvalidSubscriptionOperationException : DomainException
    {
        public InvalidSubscriptionOperationException(string reason)
            : base($"Invalid subscription operation: {reason}", "INVALID_SUB_OP") { }
    }
}