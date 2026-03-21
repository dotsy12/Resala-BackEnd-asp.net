using BackEnd.Domain.Common;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Events;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Sponsorship
{
    public sealed class Sponsorship : BaseEntity<int>, IAggregateRoot
    {
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string? ImagePath { get; private set; }
        public string? IconPath { get; private set; }
        public string Category { get; private set; } = null!;
        public SponsorshipStatus Status { get; private set; }
        public UrgencyLevel UrgencyLevel { get; private set; }
        public Money? FinancialGoal { get; private set; }
        public Money TotalCollected { get; private set; } = null!;
        public SponsorshipPolicy Policy { get; private set; } = null!;

        private Sponsorship() { }

        public static Sponsorship Create(
            string name, string description, string category,
            Money? financialGoal = null, SponsorshipPolicy? policy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.", nameof(description));

            return new Sponsorship
            {
                Name = name.Trim(),
                Description = description.Trim(),
                Category = category.Trim(),
                Status = SponsorshipStatus.Active,
                UrgencyLevel = UrgencyLevel.Normal,
                FinancialGoal = financialGoal,
                TotalCollected = Money.Zero(),
                Policy = policy ?? SponsorshipPolicy.Default,
                CreatedOn = DateTime.UtcNow
            };
        }

        public void Activate() { Status = SponsorshipStatus.Active; UpdatedOn = DateTime.UtcNow; }
        public void Deactivate() { Status = SponsorshipStatus.Inactive; UpdatedOn = DateTime.UtcNow; }

        public void SetUrgencyLevel(UrgencyLevel level)
        {
            if (UrgencyLevel == level) return;
            UrgencyLevel = level;
            UpdatedOn = DateTime.UtcNow;
            AddDomainEvent(new SponsorshipUrgencyChangedEvent(Id, level));
        }

        public void UpdatePolicy(SponsorshipPolicy policy)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            UpdatedOn = DateTime.UtcNow;
        }

        public void AddToTotalCollected(Money amount)
        {
            TotalCollected = TotalCollected.Add(amount);
            UpdatedOn = DateTime.UtcNow;
        }

        public void UpdateImages(string? imagePath, string? iconPath)
        {
            ImagePath = imagePath;
            IconPath = iconPath;
            UpdatedOn = DateTime.UtcNow;
        }

        public bool IsActive => Status == SponsorshipStatus.Active;
    }
}