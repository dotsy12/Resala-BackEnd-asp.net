using BackEnd.Domain.Common;
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
        public int? SubscriptionId { get; private set; }
        public SponsorshipSubscription? Subscription { get; private set; }
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

        /// <summary>إنشاء طلب دفع إلكتروني (VodafoneCash أو InstaPay)</summary>
        public static PaymentRequest CreateElectronic(
            int? subscriptionId, int? generalDonationId,
            Money amount, PaymentMethod method,
            string receiptImageUrl, string receiptImagePublicId, string senderPhoneNumber)
        {
            ValidateReference(subscriptionId, generalDonationId);

            if (method is not (PaymentMethod.VodafoneCash or PaymentMethod.InstaPay))
                throw new InvalidPaymentRequestException(
                    "هذا الـ Factory مخصص لـ VodafoneCash أو InstaPay فقط.");

            if (string.IsNullOrWhiteSpace(receiptImageUrl))
                throw new InvalidPaymentRequestException("صورة الإيصال مطلوبة.");
            if (string.IsNullOrWhiteSpace(receiptImagePublicId))
                throw new InvalidPaymentRequestException("معرّف صورة الإيصال مطلوب.");

            if (string.IsNullOrWhiteSpace(senderPhoneNumber))
                throw new InvalidPaymentRequestException("رقم الهاتف المُحوَّل منه مطلوب.");

            return new PaymentRequest
            {
                SubscriptionId = subscriptionId,
                GeneralDonationId = generalDonationId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Pending,
                ReceiptImageUrl = receiptImageUrl,
                ReceiptImagePublicId = receiptImagePublicId,
                SenderPhoneNumber = senderPhoneNumber.Trim(),
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <summary>إنشاء طلب دفع في الفرع</summary>
        public static PaymentRequest CreateBranch(
            int? subscriptionId, int? generalDonationId,
            Money amount, BranchPaymentDetails branchDetails)
        {
            ValidateReference(subscriptionId, generalDonationId);

            return new PaymentRequest
            {
                SubscriptionId = subscriptionId,
                GeneralDonationId = generalDonationId,
                Amount = amount,
                Method = PaymentMethod.Branch,
                Status = PaymentStatus.Pending,
                BranchDetails = branchDetails
                    ?? throw new ArgumentNullException(nameof(branchDetails)),
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <summary>إنشاء طلب مندوب</summary>
        public static PaymentRequest CreateRepresentative(
            int? subscriptionId, int? generalDonationId,
            Money amount, RepresentativeDetails repDetails)
        {
            ValidateReference(subscriptionId, generalDonationId);

            return new PaymentRequest
            {
                SubscriptionId = subscriptionId,
                GeneralDonationId = generalDonationId,
                Amount = amount,
                Method = PaymentMethod.Representative,
                Status = PaymentStatus.Pending,
                RepresentativeInfo = repDetails
                    ?? throw new ArgumentNullException(nameof(repDetails)),
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <summary>تأكيد الدفع من الـ Staff</summary>
        public void Verify(int staffId)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidPaymentRequestException(
                    $"لا يمكن تأكيد طلب بحالة '{Status}'.");

            Status = PaymentStatus.Verified;
            VerifiedByStaffId = staffId;
            VerifiedAt = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;

            AddDomainEvent(new PaymentVerifiedEvent(
                Id, SubscriptionId, GeneralDonationId,
                Amount.Amount, staffId));
        }

        /// <summary>رفض الدفع من الـ Staff</summary>
        public void Reject(int staffId, string reason)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidPaymentRequestException(
                    $"لا يمكن رفض طلب بحالة '{Status}'.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("سبب الرفض مطلوب.");

            Status = PaymentStatus.Rejected;
            VerifiedByStaffId = staffId;
            RejectionReason = reason.Trim();
            UpdatedOn = DateTime.UtcNow;
        }

        /// <summary>إلغاء الطلب من المتبرع</summary>
        public void Cancel()
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidPaymentRequestException("يمكن إلغاء الطلبات المعلقة فقط.");

            Status = PaymentStatus.Cancelled;
            UpdatedOn = DateTime.UtcNow;
        }

        private static void ValidateReference(int? subscriptionId, int? generalDonationId)
        {
            if (subscriptionId.HasValue == generalDonationId.HasValue)
                throw new InvalidPaymentRequestException(
                    "يجب ربط الطلب باشتراك أو تبرع — وليس الاثنين أو لا شيء.");
        }
    }
}