using BackEnd.Domain.Common;

namespace BackEnd.Domain.Entities.Notification
{
    public sealed class DeliveryArea : BaseEntity<int>
    {
        public string Name { get; private set; } = null!;

        private DeliveryArea() { }

        public static DeliveryArea Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Area name is required.", nameof(name));

            return new DeliveryArea
            {
                Name = name.Trim(),
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Name is required.");
            Name = newName.Trim();
            UpdatedOn = DateTime.UtcNow;
        }
    }
}