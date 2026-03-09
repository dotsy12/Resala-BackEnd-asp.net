// Domain/ValueObjects/ValueObjects.cs
namespace BackEnd.Domain.ValueObjects
{
    // ── PersonName ───────────────────────────────────────────
    public sealed class PersonName : IEquatable<PersonName>
    {
        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public string FullName => $"{FirstName} {LastName}";

        private PersonName() { }

        public PersonName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required.");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required.");

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        public bool Equals(PersonName? other) =>
            other is not null &&
            string.Equals(FirstName, other.FirstName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(LastName, other.LastName, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => Equals(obj as PersonName);
        public override int GetHashCode() =>
            HashCode.Combine(FirstName.ToLower(), LastName.ToLower());
        public override string ToString() => FullName;
    }
}