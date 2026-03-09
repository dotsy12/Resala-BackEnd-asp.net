using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Payment
{
    public sealed class GeneralDonation : BaseEntity<int>, IAggregateRoot
    {
        public int DonorId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public DonationType DonationType { get; private set; }
        public string? Note { get; private set; }

        public Donor? Donor { get; private set; }

        private GeneralDonation() { }

        public static GeneralDonation Create(
            int donorId, decimal amount,
            DonationType donationType, string? note = null)
        {
            if (amount <= 0)
                throw new Exceptions.InvalidMoneyAmountException(amount);

            return new GeneralDonation
            {
                DonorId = donorId,
                Amount = new Money(amount),
                DonationType = donationType,
                Note = note?.Trim(),
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}