namespace BackEnd.Domain.ValueObjects
{
    /// <summary>
    /// تفاصيل طلب المندوب — يحتاج عنوان + بيانات الاتصال
    /// </summary>
    public sealed class RepresentativeDetails : IEquatable<RepresentativeDetails>
    {
        public int DeliveryAreaId { get; private set; }
        public string DeliveryAreaName { get; private set; } = null!;
        public string ContactName { get; private set; } = null!;    // اسم جهة الاتصال
        public string ContactPhone { get; private set; } = null!;   // رقم جهة الاتصال
        public string Address { get; private set; } = null!;        // العنوان التفصيلي
        public string? Notes { get; private set; }

        private RepresentativeDetails() { }

        public RepresentativeDetails(
            int deliveryAreaId, string deliveryAreaName,
            string contactName, string contactPhone,
            string address, string? notes = null)
        {
            if (deliveryAreaId <= 0)
                throw new ArgumentException("منطقة التوصيل مطلوبة.");
            if (string.IsNullOrWhiteSpace(deliveryAreaName))
                throw new ArgumentException("اسم المنطقة مطلوب.");
            if (string.IsNullOrWhiteSpace(contactName))
                throw new ArgumentException("اسم جهة الاتصال مطلوب.");
            if (string.IsNullOrWhiteSpace(contactPhone))
                throw new ArgumentException("رقم جهة الاتصال مطلوب.");
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("العنوان التفصيلي مطلوب.");

            DeliveryAreaId = deliveryAreaId;
            DeliveryAreaName = deliveryAreaName.Trim();
            ContactName = contactName.Trim();
            ContactPhone = contactPhone.Trim();
            Address = address.Trim();
            Notes = notes?.Trim();
        }

        public bool Equals(RepresentativeDetails? other) =>
            other is not null && DeliveryAreaId == other.DeliveryAreaId
            && ContactPhone == other.ContactPhone;

        public override bool Equals(object? obj) => Equals(obj as RepresentativeDetails);
        public override int GetHashCode() => HashCode.Combine(DeliveryAreaId, ContactPhone);
    }
}