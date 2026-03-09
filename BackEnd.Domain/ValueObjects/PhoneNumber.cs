// Domain/ValueObjects/ValueObjects.cs
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.ValueObjects
{
    // ── PhoneNumber ──────────────────────────────────────────
    public sealed class PhoneNumber : IEquatable<PhoneNumber>
    {
        private static readonly System.Text.RegularExpressions.Regex _regex =
            new(@"^(010|011|012|015)\d{8}$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        public string Value { get; private set; } = null!;

        private PhoneNumber() { }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number is required.");

            var cleaned = value.Trim().Replace("-", "").Replace(" ", "");
            if (!_regex.IsMatch(cleaned))
                throw new InvalidPhoneNumberException(value);

            Value = cleaned;
        }

        public bool Equals(PhoneNumber? other) =>
            other is not null && Value == other.Value;

        public override bool Equals(object? obj) => Equals(obj as PhoneNumber);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
        public static implicit operator string(PhoneNumber p) => p.Value;
    }
}