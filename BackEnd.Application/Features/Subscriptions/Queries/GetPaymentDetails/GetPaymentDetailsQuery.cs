using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetPaymentDetails
{
    public record GetPaymentDetailsQuery(int PaymentId) : IRequest<Result<PaymentRequestDetailDto>>;

    public class GetPaymentDetailsHandler : IRequestHandler<GetPaymentDetailsQuery, Result<PaymentRequestDetailDto>>
    {
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly IStaffRepository _staffRepo;

        public GetPaymentDetailsHandler(IPaymentRequestRepository paymentRepo, IStaffRepository staffRepo)
        {
            _paymentRepo = paymentRepo;
            _staffRepo = staffRepo;
        }

        public async Task<Result<PaymentRequestDetailDto>> Handle(GetPaymentDetailsQuery request, CancellationToken ct)
        {
            var p = await _paymentRepo.GetByIdAsync(request.PaymentId, ct);
            if (p == null)
                return Result<PaymentRequestDetailDto>.Failure("طلب الدفع غير موجود.", ErrorType.NotFound);

            var user = p.Subscription?.Donor?.User;
            
            string? verifiedBy = null;
            if (p.VerifiedByStaffId.HasValue)
            {
                var staff = await _staffRepo.GetByIdAsync(p.VerifiedByStaffId.Value, ct);
                if (staff?.User != null)
                {
                    verifiedBy = $"{staff.User.FirstName} {staff.User.LastName}".Trim();
                }
            }

            var dto = new PaymentRequestDetailDto(
                PaymentId: p.Id,
                SubscriptionId: p.SubscriptionId,
                UserId: user?.Id ?? "",
                UserName: user != null ? $"{user.FirstName} {user.LastName}".Trim() : "",
                Phone: user?.PhoneNumber ?? "",
                PaymentMethod: p.Method.ToString(),
                Amount: p.Amount.Amount,
                Status: p.Status.ToString(),
                CreatedAt: p.CreatedOn,
                ReceiptImageUrl: p.ReceiptImageUrl,
                ReceiptPublicId: p.ReceiptImagePublicId,
                Notes: null, // Add Notes field to PaymentRequest entity if desired
                SenderPhoneNumber: p.SenderPhoneNumber,
                ScheduledDate: p.BranchDetails?.ScheduledDate,
                Address: p.RepresentativeInfo?.Address,
                VerifiedBy: verifiedBy,
                VerifiedAt: p.VerifiedAt,
                RejectionReason: p.RejectionReason
            );

            return Result<PaymentRequestDetailDto>.Success(dto);
        }
    }
}
