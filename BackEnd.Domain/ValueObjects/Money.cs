// Domain/ValueObjects/ValueObjects.cs
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.ValueObjects
{
    // ── Money ────────────────────────────────────────────────
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = null!;

        private Money() { }   // EF Core

        public Money(decimal amount, string currency = "EGP")
        {
            if (amount < 0)
                throw new InvalidMoneyAmountException(amount);

            Amount = Math.Round(amount, 2);
            Currency = currency.ToUpperInvariant();
        }

        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount - other.Amount, Currency);
        }

        public static Money Zero(string currency = "EGP") => new(0, currency);

        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException(
                    $"Cannot operate on {Currency} and {other.Currency}");
        }

        public bool Equals(Money? other) =>
            other is not null &&
            Amount == other.Amount && Currency == other.Currency;

        public override bool Equals(object? obj) => Equals(obj as Money);
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
        public override string ToString() => $"{Amount:F2} {Currency}";
    }
}