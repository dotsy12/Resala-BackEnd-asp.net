using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Application.Dtos.EmergencyCase
{
    /// <summary>بيانات التبرع الإلكتروني (فودافون كاش / إنستا باي)</summary>
    public class ElectronicEmergencyDonationDto
    {
        [Range(1, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        /// <summary>1 = VodafoneCash, 2 = InstaPay</summary>
        [Range(1, 2, ErrorMessage = "طريقة الدفع يجب أن تكون 1 (VodafoneCash) أو 2 (InstaPay)")]
        public int PaymentMethod { get; set; }

        [Required(ErrorMessage = "رقم الهاتف المحول منه مطلوب")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف غير صحيح")]
        public string SenderPhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "صورة الإيصال مطلوبة")]
        public IFormFile ReceiptImage { get; set; } = null!;
    }

    /// <summary>بيانات التبرع عن طريق مندوب</summary>
    public class RepresentativeEmergencyDonationDto
    {
        [Range(1, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "منطقة التوصيل مطلوبة")]
        public int DeliveryAreaId { get; set; }

        [Required(ErrorMessage = "العنوان مطلوب")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "اسم الشخص للتواصل مطلوب")]
        public string ContactName { get; set; } = null!;

        [Required(ErrorMessage = "رقم هاتف للتواصل مطلوب")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف غير صحيح")]
        public string ContactPhone { get; set; } = null!;

        public string? RepresentativeNotes { get; set; }
    }

    /// <summary>بيانات التبرع في الفرع</summary>
    public class BranchEmergencyDonationDto
    {
        [Range(1, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "الموعد مطلوب")]
        public int SlotId { get; set; }

        [Required(ErrorMessage = "رقم هاتف للتواصل مطلوب")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف غير صحيح")]
        public string BranchContactPhone { get; set; } = null!;
    }
}
