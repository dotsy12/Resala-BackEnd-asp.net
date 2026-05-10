// RejectPaymentCommand.cs + Handler
using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.RejectPayment
{
    public class RejectPaymentHandler
        : IRequestHandler<RejectPaymentCommand, Result<string>>
    {
        private readonly IPaymentRequestRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RejectPaymentHandler> _logger;

        public RejectPaymentHandler(
            IPaymentRequestRepository repo,
            IUnitOfWork unitOfWork,
            ILogger<RejectPaymentHandler> logger)
        { 
            _repo = repo; 
            _unitOfWork = unitOfWork;
            _logger = logger; 
        }

        public async Task<Result<string>> Handle(
            RejectPaymentCommand request, CancellationToken ct)
        {
            var payment = await _repo.GetByIdAsync(request.PaymentId, ct);
            if (payment is null)
                return Result<string>.Failure("طلب الدفع غير موجود.", ErrorType.NotFound);

            payment.Reject(request.StaffId, request.Reason);
            _repo.Update(payment);

            // يتم إرسال الإشعار تلقائياً عبر Domain Event Handler (PaymentRejectedEventHandler)
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("تم رفض الدفع: Id={Id}", payment.Id);
            return Result<string>.Success("تم رفض طلب الدفع وإرسال إشعار للمتبرع.");
        }
    }
}
