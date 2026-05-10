using BackEnd.Domain.Enums;

namespace BackEnd.Application.Dtos.PaymentNumber
{
    public class PaymentNumberDto
    {
        public int Id { get; set; }
        public PaymentMethod Type { get; set; }
        public string TypeName => Type.ToString();
        public string Number { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpsertPaymentNumberDto
    {
        public PaymentMethod Type { get; set; }
        public string Number { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
