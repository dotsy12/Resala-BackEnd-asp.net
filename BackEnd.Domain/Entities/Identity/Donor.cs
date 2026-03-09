// Domain/Entities/Identity/Donor.cs
using BackEnd.Domain.Common;
using BackEnd.Domain.Events;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Identity
{
    public sealed class Donor : BaseEntity<int>, IAggregateRoot
    {
        public string UserId { get; private set; } = null!;
        public PersonName FullName { get; private set; } = null!;
        public EmailAddress Email { get; private set; } = null!;
        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public string? Landline { get; private set; }
        public string? Job { get; private set; }

        // Navigation (read-only from outside)
        public ApplicationUser? User { get; private set; }

        private Donor() { }

        public static Donor Create(
            string userId,
            string firstName, string lastName,
            string email, string phoneNumber,
            string? job = null, string? landline = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var donor = new Donor
            {
                UserId = userId,
                FullName = new PersonName(firstName, lastName),
                Email = new EmailAddress(email),
                PhoneNumber = new PhoneNumber(phoneNumber),
                Job = job?.Trim(),
                Landline = landline?.Trim(),
                CreatedOn = DateTime.UtcNow
            };

            donor.AddDomainEvent(new DonorRegisteredEvent(0, email));
            return donor;
        }

        public void UpdateProfile(
            string firstName, string lastName,
            string? job, string? landline)
        {
            FullName = new PersonName(firstName, lastName);
            Job = job?.Trim();
            Landline = landline?.Trim();
            UpdatedOn = DateTime.UtcNow;
        }
     
        public void SetUserId(string userId)
        {
            UserId = userId;
        }
    }
}