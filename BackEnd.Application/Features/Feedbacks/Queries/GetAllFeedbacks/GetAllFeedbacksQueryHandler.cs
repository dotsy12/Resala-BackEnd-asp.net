using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Feedback;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Feedbacks.Queries.GetAllFeedbacks
{
    public class GetAllFeedbacksQueryHandler : IRequestHandler<GetAllFeedbacksQuery, Result<PagedResult<FeedbackDto>>>
    {
        private readonly IFeedbackRepository _repository;

        public GetAllFeedbacksQueryHandler(IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<PagedResult<FeedbackDto>>> Handle(GetAllFeedbacksQuery request, CancellationToken ct)
        {
            var (items, total) = await _repository.GetAllPagedAsync(
                request.Type, request.Status, request.PageNumber, request.PageSize, ct);

            var dtoList = items.Select(f => new FeedbackDto
            {
                Id = f.Id,
                DonorId = f.DonorId,
                DonorName = f.Donor != null ? $"{f.Donor.FullName.FirstName} {f.Donor.FullName.LastName}" : "Unknown",
                Subject = f.Subject,
                Message = f.Message,
                Type = f.Type,
                Status = f.Status,
                CreatedOn = f.CreatedOn
            }).ToList();

            var pagedResult = new PagedResult<FeedbackDto>(
                dtoList, total, request.PageNumber, request.PageSize);

            return Result<PagedResult<FeedbackDto>>.Success(pagedResult);
        }
    }
}
