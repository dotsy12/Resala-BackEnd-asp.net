using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.EmergencyCase;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Entities.Sponsorship;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Events;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Payment
{
    public sealed class PaymentRequest : BaseEntity<int>, IAggregateRoot
    {
        public int DonorId { get; private set; }
        public Donor? Donor { get; private set; }

        public PaymentTargetType TargetType { get; private set; }
        public int? TargetId { get; private set; } // Generic ID to support various targets if needed

        public int? SubscriptionId { get; private set; }
        public SponsorshipSubscription? Subscription { get; private set; }
        
        public int? EmergencyCaseId { get; private set; }
        public EmergencyCase.EmergencyCase? EmergencyCase { get; private set; }

        public int? GeneralDonationId { get; private set; }
        
        public Money Amount { get; private set; } = null!;
        public PaymentMethod Method { get; private set; }
        public PaymentStatus Status { get; private set; }

        // VodafoneCash / InstaPay
        public string? ReceiptImageUrl { get; private set; }
        public string? ReceiptImagePublicId { get; private set; }
        public string? ReceiptImagePath => ReceiptImageUrl;
        public string? SenderPhoneNumber { get; private set; }

        // Branch
        public BranchPaymentDetails? BranchDetails { get; private set; }

        // Representative
        public RepresentativeDetails? RepresentativeInfo { get; private set; }

        // Staff review
        public int? VerifiedByStaffId { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public string? RejectionReason { get; private set; }

        private PaymentRequest() { }

        private static PaymentRequest CreateBase(
            int donorId, Money amount, PaymentMethod method,
            int? subscriptionId = null, int? emergencyCaseId = null, int? generalDonationId = null)
        {
            ValidateReference(subscriptionId, emergencyCaseId, generalDonationId);

            var targetType = subscriptionId.HasValue ? PaymentTargetType.Subscription 
                           : emergencyCaseId.HasValue ? PaymentTargetType.EmergencyCase 
                           : PaymentTargetType.GeneralDonation;

            return new PaymentRequest
            {
                DonorId = donorId,
                TargetType = targetType,
                TargetId = subscriptionId ?? emergencyCaseId ?? generalDonationId,
                SubscriptionId = subscriptionId,
                EmergencyCaseId = emergencyCaseId,
                GeneralDonationId = generalDonationId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <summary>إنشاء طلب دفع إلكتروني (VodafoneCash أو InstaPay)</summary>
        public static PaymentRequest CreateElectronic(
            int donorId, int? subscriptionId, int? emergencyCaseId, int? generalDonationId,
            Money amount, PaymentMethod method,
            string receiptImageUrl, string receiptImagePublicId, string senderPhoneNumber)
        {
            if (method is not (PaymentMethod.VodafoneCash or PaymentMethod.InstaPay))
                throw new InvalidPaymentRequestException("هذا الـ Factory مخصص لـ VodafoneCash أو InstaPay فقط.");

            if (string.IsNullOrWhiteSpace(receiptImageUrl))
                throw new InvalidPaymentRequestException("صورة الإيصال مطلوبة.");
            if (string.IsNullOrWhiteSpace(receiptImagePublicId))
                throw new InvalidPaymentRequestException("معرّف صورة الإيصال مطلوب.");
            if (string.IsNullOrWhiteSpace(senderPhoneNumber))
                throw new InvalidPaymentRequestException("رقم الهاتف المُحوَّل منه مطلوب.");

            var payment = CreateBase(donorId, amount, method, subscriptionId, emergencyCaseId, generalDonationId);
            payment.ReceiptImageUrl = receiptImageUrl;
            payment.ReceiptImagePublicId = receiptImagePublicId;
            payment.SenderPhoneNumber = senderPhoneNumber.Trim();
            
            return payment;
        }

        /// <summary>إنشاء طلب دفع في الفرع</summary>
        public static PaymentRequest CreateBranch(
            int donorId, int? subscriptionId, int? emergencyCaseId, int? generalDonationId,
            Money amount, BranchPaymentDetails branchDetails)
        {
            var payment = CreateBase(donorId, amount, PaymentMethod.Branch, subscriptionId, emergencyCaseId, generalDonationId);
            payment.BranchDetails = branchDetails ?? throw new ArgumentNullException(nameof(branchDetails));
            return payment;
        }

        /// <summary>إنشاء طلب مندوب</summary>
        public static PaymentRequest CreateRepresentative(
            int donorId, int? subscriptionId, int? emergencyCaseId, int? generalDonationId,
            Money amount, RepresentativeDetails repDetails)
        {
            var payment = CreateBase(donorId, amount, PaymentMethod.Representative, subscriptionId, emergencyCaseId, generalDonationId);
            payment.RepresentativeInfo = repDetails ?? throw new ArgumentNullException(nameof(repDetails));
            return payment;
        }

        /// <summary>تأكيد الدفع من الـ Staff</summary>
        public void Verify(int staffId)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidPaymentRequestException($"لا يمكن تأكيد طلب بحالة '{Status}'.");

            Status = PaymentStatus.Verified;
            VerifiedByStaffId = staffId;
            VerifiedAt = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;

            AddDomainEvent(new PaymentVerifiedEvent(
                Id, SubscriptionId, GeneralDonationId, EmergencyCaseId,
                Amount.Amount, staffId));
        }

        /// <summary>رفض الدفع من الـ Staff</summary>
        public void Reject(int staffId, string reason)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidPaymentRequestException($"لا يمكن رفض طلب بحالة '{Status}'.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("سبب الرفض مطلوب.");

            Status = PaymentStatus.Rejected;
            VerifiedByStaffId = staffId;
            RejectionReason = reason.Trim();
            UpdatedOn = DateTime.UtcNow;

            AddDomainEvent(new PaymentRejectedEvent(Id, DonorId, RejectionReason));
        }

        private static void ValidateReference(int? subscriptionId, int? emergencyCaseId, int? generalDonationId)
        {
            int count = 0;
            if (subscriptionId.HasValue) count++;
            if (emergencyCaseId.HasValue) count++;
            if (generalDonationId.HasValue) count++;

            if (count != 1)
                throw new InvalidPaymentRequestException("يجب ربط الطلب بهدف واحد فقط (اشتراك أو حالة طوارئ أو تبرع عام).");
        }
    }
}
