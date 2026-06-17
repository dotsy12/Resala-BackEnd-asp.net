using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus
{
    public class UpdateFeedbackStatusCommandHandler : IRequestHandler<UpdateFeedbackStatusCommand, Result<bool>>
    {
        private readonly IFeedbackRepository _repository;
        private readonly ILogger<UpdateFeedbackStatusCommandHandler> _logger;

        public UpdateFeedbackStatusCommandHandler(
            IFeedbackRepository repository,
            ILogger<UpdateFeedbackStatusCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateFeedbackStatusCommand request, CancellationToken ct)
        {
            var feedback = await _repository.GetByIdAsync(request.Id, ct);
            if (feedback == null)
                return Result<bool>.Failure("الفيدباك غير موجودة.", ErrorType.NotFound);

            feedback.ChangeStatus(request.Status);
            await _repository.SaveChangesAsync(ct);

            _logger.LogInformation("Feedback {Id} status updated to {Status}", feedback.Id, request.Status);

            return Result<bool>.Success(true, "تم تحديث حالة الفيدباك بنجاح.");
        }
    }
}
