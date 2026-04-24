// VerifyPaymentCommand.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.VerifyPayment
{
    public class VerifyPaymentHandler
        : IRequestHandler<VerifyPaymentCommand, Result<string>>
    {
        private readonly IPaymentRequestRepository _paymentRepo;
        private readonly ISponsorshipSubscriptionRepository _subRepo;
        private readonly ILogger<VerifyPaymentHandler> _logger;

        public VerifyPaymentHandler(
            IPaymentRequestRepository paymentRepo,
            ISponsorshipSubscriptionRepository subRepo,
            ILogger<VerifyPaymentHandler> logger)
        { _paymentRepo = paymentRepo; _subRepo = subRepo; _logger = logger; }

        public async Task<Result<string>> Handle(
            VerifyPaymentCommand request, CancellationToken ct)
        {
            var payment = await _paymentRepo.GetByIdAsync(request.PaymentId, ct);
            if (payment is null)
                return Result<string>.Failure("طلب الدفع غير موجود.", ErrorType.NotFound);

            payment.Verify(request.StaffId);
            _paymentRepo.Update(payment);

            // تحديث تاريخ الدفعة القادمة تلقائياً
            if (payment.SubscriptionId.HasValue)
            {
                var sub = await _subRepo.GetByIdAsync(payment.SubscriptionId.Value, ct);
                if (sub is not null)
                {
                    sub.AdvancePaymentDate();
                    _subRepo.Update(sub);
                }
            }

            await _paymentRepo.SaveChangesAsync(ct);

            _logger.LogInformation(
                "تم تأكيد الدفع: Id={Id} بواسطة Staff={StaffId}",
                payment.Id, request.StaffId);

            return Result<string>.Success("تم تأكيد الدفع بنجاح. تم تحديث تاريخ الدفعة القادمة.");
        }
    }
}