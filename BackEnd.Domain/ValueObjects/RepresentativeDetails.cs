// Domain/ValueObjects/ValueObjects.cs
namespace BackEnd.Domain.ValueObjects
{
    // ── RepresentativeDetails ────────────────────────────────
    public sealed class RepresentativeDetails : IEquatable<RepresentativeDetails>
    {
        public int DeliveryAreaId { get; private set; }
        public string DeliveryAreaName { get; private set; } = null!;
        public string? Notes { get; private set; }

        private RepresentativeDetails() { }

        public RepresentativeDetails(
            int deliveryAreaId, string deliveryAreaName, string? notes = null)
        {
            if (deliveryAreaId <= 0)
                throw new ArgumentException("Delivery area is required.");
            if (string.IsNullOrWhiteSpace(deliveryAreaName))
                throw new ArgumentException("Delivery area name is required.");

            DeliveryAreaId = deliveryAreaId;
            DeliveryAreaName = deliveryAreaName.Trim();
            Notes = notes?.Trim();
        }

        public bool Equals(RepresentativeDetails? other) =>
            other is not null && DeliveryAreaId == other.DeliveryAreaId;

        public override bool Equals(object? obj) => Equals(obj as RepresentativeDetails);
        public override int GetHashCode() => DeliveryAreaId.GetHashCode();
    }
}