using BackEnd.Domain.Common;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Domain.Entities.EmergencyCase
{
    public sealed class EmergencyCase : BaseEntity<int>, IAggregateRoot
    {
        public string Title { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string? ImagePath { get; private set; }
        public string? ImagePublicId { get; private set; }

        public UrgencyLevel UrgencyLevel { get; private set; }

        public Money RequiredAmount { get; private set; } = null!;
        public Money CollectedAmount { get; private set; } = null!;

        public bool IsActive { get; private set; }

        private EmergencyCase() { }

        // ✅ Factory Method
        public static EmergencyCase Create(
            string title,
            string description,
            UrgencyLevel urgencyLevel,
            Money requiredAmount,
            string? imagePath = null,
            string? imagePublicId = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.");

            return new EmergencyCase
            {
                Title = title.Trim(),
                Description = description.Trim(),
                UrgencyLevel = urgencyLevel,
                RequiredAmount = requiredAmount,
                CollectedAmount = Money.Zero(),
                ImagePath = imagePath,
                ImagePublicId = imagePublicId,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };
        }

        // ✅ Business Methods

        public void UpdateDetails(string title, string description, string? imagePath, string? imagePublicId)
        {
            Title = string.IsNullOrWhiteSpace(title)
                ? throw new ArgumentException("Title is required.")
                : title.Trim();

            Description = string.IsNullOrWhiteSpace(description)
                ? throw new ArgumentException("Description is required.")
                : description.Trim();

            ImagePath = imagePath;
            ImagePublicId = imagePublicId;
            UpdatedOn = DateTime.UtcNow;
        }

        public void UpdateRequiredAmount(Money requiredAmount)
        {
            if (requiredAmount == null)
                throw new ArgumentNullException(nameof(requiredAmount));

            RequiredAmount = requiredAmount;
            UpdatedOn = DateTime.UtcNow;
        }
        public void SetUrgency(UrgencyLevel level)
        {
            UrgencyLevel = level;
            UpdatedOn = DateTime.UtcNow;
        }

        public void AddDonation(Money amount)
        {
            CollectedAmount = CollectedAmount.Add(amount);
            UpdatedOn = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedOn = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedOn = DateTime.UtcNow;
        }

        public bool IsCompleted =>
            RequiredAmount is not null &&
            CollectedAmount.Amount >= RequiredAmount.Amount;
    }
}
