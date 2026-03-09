// Domain/ValueObjects/ValueObjects.cs
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.ValueObjects
{
    // ── EmailAddress ─────────────────────────────────────────
    public sealed class EmailAddress : IEquatable<EmailAddress>
    {
        private static readonly System.Text.RegularExpressions.Regex _regex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                System.Text.RegularExpressions.RegexOptions.Compiled |
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public string Value { get; private set; } = null!;

        private EmailAddress() { }

        public EmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email is required.");
            if (!_regex.IsMatch(value))
                throw new InvalidEmailException(value);

            Value = value.Trim().ToLowerInvariant();
        }

        public bool Equals(EmailAddress? other) =>
            other is not null &&
            string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => Equals(obj as EmailAddress);
        public override int GetHashCode() => Value.ToLower().GetHashCode();
        public override string ToString() => Value;
        public static implicit operator string(EmailAddress e) => e.Value;
    }
}