using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Feedback;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Feedbacks.Commands.SubmitFeedback
{
    public class SubmitFeedbackCommandHandler : IRequestHandler<SubmitFeedbackCommand, Result<FeedbackDto>>
    {
        private readonly IFeedbackRepository _repository;
        private readonly IDonorRepository _donorRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SubmitFeedbackCommandHandler> _logger;

        public SubmitFeedbackCommandHandler(
            IFeedbackRepository repository,
            IDonorRepository donorRepo,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SubmitFeedbackCommandHandler> logger)
        {
            _repository = repository;
            _donorRepo = donorRepo;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Result<FeedbackDto>> Handle(SubmitFeedbackCommand request, CancellationToken ct)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Result<FeedbackDto>.Failure("المستخدم غير مصرح له.", ErrorType.Unauthorized);

            var donorId = await _donorRepo.GetIdByUserIdAsync(userId, ct);
            if (donorId == null)
                return Result<FeedbackDto>.Failure("لم يتم العثور على بيانات المتبرع.", ErrorType.NotFound);

            var feedback = Feedback.Create(
                donorId.Value,
                request.Subject,
                request.Message,
                request.Type);

            await _repository.AddAsync(feedback, ct);
            await _repository.SaveChangesAsync(ct);

            _logger.LogInformation("Feedback submitted by Donor {DonorId} (Type: {Type})", donorId, request.Type);

            return Result<FeedbackDto>.Success(new FeedbackDto
            {
                Id = feedback.Id,
                DonorId = feedback.DonorId,
                Subject = feedback.Subject,
                Message = feedback.Message,
                Type = feedback.Type,
                Status = feedback.Status,
                CreatedOn = feedback.CreatedOn
            });
        }
    }
}
