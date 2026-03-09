// Domain/Entities/Identity/StaffMember.cs
using BackEnd.Domain.Common;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Identity
{
    public sealed class StaffMember : BaseEntity<int>, IAggregateRoot
    {
        public string UserId { get; private set; } = null!;
        public PersonName FullName { get; private set; } = null!;
        public string Username { get; private set; } = null!;
        public EmailAddress Email { get; private set; } = null!;
        public PhoneNumber Phone { get; private set; } = null!;
        public StaffType StaffType { get; private set; }
        public AccountStatus AccountStatus { get; private set; }

        public ApplicationUser? User { get; private set; }

        private StaffMember() { }

        public static StaffMember Create(
            string userId,
            string firstName, string lastName,
            string username, string email, string phone,
            StaffType staffType)
        {
            return new StaffMember
            {
                UserId = userId,
                FullName = new PersonName(firstName, lastName),
                Username = username.Trim(),
                Email = new EmailAddress(email),
                Phone = new PhoneNumber(phone),
                StaffType = staffType,
                AccountStatus = AccountStatus.Active,
                CreatedOn = DateTime.UtcNow
            };
        }

        public void SetAccountStatus(AccountStatus status)
        {
            AccountStatus = status;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}