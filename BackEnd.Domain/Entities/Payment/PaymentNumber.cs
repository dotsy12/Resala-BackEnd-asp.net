using BackEnd.Domain.Common;
using BackEnd.Domain.Enums;
using System;

namespace BackEnd.Domain.Entities.Payment
{
    public sealed class PaymentNumber : BaseEntity<int>
    {
        public PaymentMethod Type { get; private set; }
        public string Number { get; private set; } = null!;

        private PaymentNumber() { }

        public static PaymentNumber Create(PaymentMethod type, string number, bool isActive = true)
        {
            return new PaymentNumber
            {
                Type = type,
                Number = number.Trim(),
                IsActive = isActive,
                CreatedOn = DateTime.UtcNow
            };
        }

        public void Update(string number, bool isActive)
        {
            Number = number.Trim();
            IsActive = isActive;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
