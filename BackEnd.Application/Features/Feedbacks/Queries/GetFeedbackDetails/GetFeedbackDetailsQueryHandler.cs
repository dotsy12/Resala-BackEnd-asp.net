using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Feedback;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Feedbacks.Queries.GetFeedbackDetails
{
    public class GetFeedbackDetailsQueryHandler : IRequestHandler<GetFeedbackDetailsQuery, Result<FeedbackDto>>
    {
        private readonly IFeedbackRepository _repository;

        public GetFeedbackDetailsQueryHandler(IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<FeedbackDto>> Handle(GetFeedbackDetailsQuery request, CancellationToken ct)
        {
            var f = await _repository.GetByIdAsync(request.Id, ct);

            if (f == null)
                return Result<FeedbackDto>.Failure("الفيدباك غير موجودة.", ErrorType.NotFound);

            return Result<FeedbackDto>.Success(new FeedbackDto
            {
                Id = f.Id,
                DonorId = f.DonorId,
                DonorName = f.Donor != null ? $"{f.Donor.FullName.FirstName} {f.Donor.FullName.LastName}" : "Unknown",
                Subject = f.Subject,
                Message = f.Message,
                Type = f.Type,
                Status = f.Status,
                CreatedOn = f.CreatedOn
            });
        }
    }
}
