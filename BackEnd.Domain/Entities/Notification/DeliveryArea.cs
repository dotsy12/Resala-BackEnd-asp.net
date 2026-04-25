using BackEnd.Domain.Common;

namespace BackEnd.Domain.Entities.Notification
{
    public sealed class DeliveryArea : BaseEntity<int>
    {
        public string Name { get; private set; } = null!;
        public string Governorate { get; private set; } = null!;
        public string City { get; private set; } = null!;
        public bool IsActive { get; private set; }

        private DeliveryArea() { }

        public static DeliveryArea Create(string name, string governorate, string city)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));
            
            if (string.IsNullOrWhiteSpace(governorate))
                throw new ArgumentException("Governorate is required.", nameof(governorate));
            
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required.", nameof(city));

            return new DeliveryArea
            {
                Name = name.Trim(),
                Governorate = governorate.Trim(),
                City = city.Trim(),
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };
        }

        public void Update(string name, string governorate, string city, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.");
            
            Name = name.Trim();
            Governorate = governorate.Trim();
            City = city.Trim();
            IsActive = isActive;
            UpdatedOn = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
