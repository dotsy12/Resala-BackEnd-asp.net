using BackEnd.Application.Common.Files;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Cart;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Entities.Sponsorship;
using BackEnd.Domain.Enums;
using BackEnd.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Cart.Commands.CheckoutCart
{
    public class CheckoutCartHandler : IRequestHandler<CheckoutCartCommand, Result<bool>>
    {
        private readonly ICartRepository _cartRepo;
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly ISponsorshipRepository _sponsorshipRepo;
        private readonly IEmergencyCaseRepository _emergencyCaseRepo;
        private readonly IDeliveryAreaRepository _deliveryAreaRepo;
        private readonly IAppointmentSlotRepository _slotRepo;
        private readonly IFileUploadService _fileUploadService;

        public CheckoutCartHandler(
            ICartRepository cartRepo,
            IPaymentRequestRepository paymentRepo,
            ISponsorshipSubscriptionRepository subRepo,
            ISponsorshipRepository sponsorshipRepo,
            IEmergencyCaseRepository emergencyCaseRepo,
            IDeliveryAreaRepository deliveryAreaRepo,
            IAppointmentSlotRepository slotRepo,
            IFileUploadService fileUploadService)
        {
            _cartRepo = cartRepo;
            _paymentRepo = paymentRepo;
            _subRepo = subRepo;
            _sponsorshipRepo = sponsorshipRepo;
            _emergencyCaseRepo = emergencyCaseRepo;
            _deliveryAreaRepo = deliveryAreaRepo;
            _slotRepo = slotRepo;
            _fileUploadService = fileUploadService;
        }

        public async Task<Result<bool>> Handle(CheckoutCartCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            // 1. Get cart items
            var cartItems = await _cartRepo.GetByDonorIdAsync(request.DonorId, ct);
            if (!cartItems.Any())
                return Result<bool>.Failure("سلتك فارغة حالياً.", ErrorType.BadRequest);

            // 2. Validate all items before processing
            foreach (var item in cartItems)
            {
                if (item.SponsorshipId.HasValue)
                {
                    var sponsorship = await _sponsorshipRepo.GetByIdAsync(item.SponsorshipId.Value, ct);
                    if (sponsorship == null)
                        return Result<bool>.Failure($"برنامج الكفالة المرتبط بعنصر السلة {item.Id} غير موجود.", ErrorType.NotFound);
                    if (!sponsorship.IsActive)
                        return Result<bool>.Failure($"برنامج الكفالة '{sponsorship.Name}' غير نشط حالياً.", ErrorType.BadRequest);
                }
                else if (item.EmergencyCaseId.HasValue)
                {
                    var emergencyCase = await _emergencyCaseRepo.GetByIdAsync(item.EmergencyCaseId.Value, ct);
                    if (emergencyCase == null)
                        return Result<bool>.Failure($"حالة الطوارئ المرتبطة بعنصر السلة {item.Id} غير موجودة.", ErrorType.NotFound);
                    if (!emergencyCase.IsActive)
                        return Result<bool>.Failure($"حالة الطوارئ '{emergencyCase.Title}' غير نشطة حالياً.", ErrorType.BadRequest);
                }
            }

            // 3. Process payment method setup
            string? receiptUrl = null;
            string? receiptPublicId = null;
            RepresentativeDetails? repDetails = null;
            BranchPaymentDetails? branchDetails = null;

            if (dto.PaymentMethod is PaymentMethod.VodafoneCash or PaymentMethod.InstaPay)
            {
                if (dto.ReceiptImage == null)
                    return Result<bool>.Failure("صورة الإيصال مطلوبة للدفع الإلكتروني.", ErrorType.BadRequest);
                if (string.IsNullOrWhiteSpace(dto.SenderPhoneNumber))
                    return Result<bool>.Failure("رقم الهاتف المحول منه مطلوب للدفع الإلكتروني.", ErrorType.BadRequest);

                var uploadResult = await _fileUploadService.UploadAsync(
                    dto.ReceiptImage, "payment-receipts", UploadContentType.Image, ct);

                if (!uploadResult.IsSuccess)
                    return Result<bool>.Failure(uploadResult.Message, ErrorType.InternalServerError);

                receiptUrl = uploadResult.Value.Url;
                receiptPublicId = uploadResult.Value.PublicId;
            }
            else if (dto.PaymentMethod == PaymentMethod.Representative)
            {
                if (!dto.DeliveryAreaId.HasValue)
                    return Result<bool>.Failure("منطقة التوصيل مطلوبة لطلب المندوب.", ErrorType.BadRequest);
                if (string.IsNullOrWhiteSpace(dto.ContactName) || string.IsNullOrWhiteSpace(dto.ContactPhone) || string.IsNullOrWhiteSpace(dto.Address))
                    return Result<bool>.Failure("بيانات الاتصال والعنوان كاملة مطلوبة لطلب المندوب.", ErrorType.BadRequest);

                var area = await _deliveryAreaRepo.GetByIdAsync(dto.DeliveryAreaId.Value, ct);
                if (area is null)
                    return Result<bool>.Failure("منطقة التوصيل غير موجودة.", ErrorType.NotFound);

                repDetails = new RepresentativeDetails(
                    deliveryAreaId: area.Id,
                    deliveryAreaName: area.Name,
                    contactName: dto.ContactName,
                    contactPhone: dto.ContactPhone,
                    address: dto.Address,
                    notes: dto.RepresentativeNotes
                );
            }
            else if (dto.PaymentMethod == PaymentMethod.Branch)
            {
                if (!dto.SlotId.HasValue)
                    return Result<bool>.Failure("الموعد مطلوب للدفع في الفرع.", ErrorType.BadRequest);
                if (string.IsNullOrWhiteSpace(dto.DonorName) || string.IsNullOrWhiteSpace(dto.BranchContactPhone))
                    return Result<bool>.Failure("اسم المتبرع ورقم الهاتف مطلوبان للدفع في الفرع.", ErrorType.BadRequest);

                var slot = await _slotRepo.GetByIdAsync(dto.SlotId.Value, ct);
                if (slot is null)
                    return Result<bool>.Failure("الموعد المختار غير موجود.", ErrorType.NotFound);

                if (!slot.HasAvailableCapacity)
                    return Result<bool>.Failure("هذا الموعد ممتلئ، اختر موعداً آخر.", ErrorType.Conflict);

                slot.Book();
                _slotRepo.Update(slot);

                branchDetails = new BranchPaymentDetails(
                    donorName: dto.DonorName,
                    contactNumber: dto.BranchContactPhone,
                    scheduledDate: slot.SlotDate.Add(slot.OpenFrom),
                    slotId: slot.Id
                );
            }
            else
            {
                return Result<bool>.Failure("طريقة الدفع المحددة غير مدعومة.", ErrorType.BadRequest);
            }

            // 4. Generate Payment Requests & Subscriptions
            foreach (var item in cartItems)
            {
                PaymentRequest paymentRequest;

                if (item.EmergencyCaseId.HasValue)
                {
                    paymentRequest = dto.PaymentMethod switch
                    {
                        PaymentMethod.VodafoneCash or PaymentMethod.InstaPay => PaymentRequest.CreateElectronic(
                            request.DonorId, null, item.EmergencyCaseId.Value, null,
                            item.DonationAmount, dto.PaymentMethod, receiptUrl!, receiptPublicId!, dto.SenderPhoneNumber!),

                        PaymentMethod.Representative => PaymentRequest.CreateRepresentative(
                            request.DonorId, null, item.EmergencyCaseId.Value, null,
                            item.DonationAmount, repDetails!),

                        PaymentMethod.Branch => PaymentRequest.CreateBranch(
                            request.DonorId, null, item.EmergencyCaseId.Value, null,
                            item.DonationAmount, branchDetails!),

                        _ => throw new InvalidOperationException()
                    };
                }
                else if (item.SponsorshipId.HasValue)
                {
                    // Find or create subscription
                    var sponsorship = (await _sponsorshipRepo.GetByIdAsync(item.SponsorshipId.Value, ct))!;
                    var subscription = await _subRepo.GetActiveByDonorAndSponsorshipAsync(
                        request.DonorId, item.SponsorshipId.Value, ct);

                    if (subscription == null)
                    {
                        subscription = SponsorshipSubscription.Create(
                            request.DonorId, item.SponsorshipId.Value, sponsorship,
                            item.DonationAmount, PaymentCycle.Monthly);

                        await _subRepo.AddAsync(subscription, ct);
                        await _subRepo.SaveChangesAsync(ct); // Ensure subscription ID is generated
                    }

                    paymentRequest = dto.PaymentMethod switch
                    {
                        PaymentMethod.VodafoneCash or PaymentMethod.InstaPay => PaymentRequest.CreateElectronic(
                            request.DonorId, subscription.Id, null, null,
                            item.DonationAmount, dto.PaymentMethod, receiptUrl!, receiptPublicId!, dto.SenderPhoneNumber!),

                        PaymentMethod.Representative => PaymentRequest.CreateRepresentative(
                            request.DonorId, subscription.Id, null, null,
                            item.DonationAmount, repDetails!),

                        PaymentMethod.Branch => PaymentRequest.CreateBranch(
                            request.DonorId, subscription.Id, null, null,
                            item.DonationAmount, branchDetails!),

                        _ => throw new InvalidOperationException()
                    };
                }
                else
                {
                    continue;
                }

                await _paymentRepo.AddAsync(paymentRequest, ct);
            }

            // 5. Clear the cart
            _cartRepo.RemoveRange(cartItems);

            // 6. Save all database changes
            await _cartRepo.SaveChangesAsync(ct);

            return Result<bool>.Success(true, "تم إتمام عملية الدفع بنجاح وتوليد طلبات الدفع المقابلة.");
        }
    }
}
