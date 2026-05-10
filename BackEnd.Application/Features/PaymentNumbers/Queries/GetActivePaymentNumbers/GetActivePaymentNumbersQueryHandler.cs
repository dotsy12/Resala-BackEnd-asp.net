using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.PaymentNumber;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.PaymentNumbers.Queries.GetActivePaymentNumbers
{
    public class GetActivePaymentNumbersQueryHandler : IRequestHandler<GetActivePaymentNumbersQuery, Result<IReadOnlyList<PaymentNumberDto>>>
    {
        private readonly IPaymentNumberRepository _repository;

        public GetActivePaymentNumbersQueryHandler(IPaymentNumberRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<PaymentNumberDto>>> Handle(GetActivePaymentNumbersQuery request, CancellationToken ct)
        {
            var numbers = await _repository.GetActiveAsync(ct);

            var dtoList = numbers.Select(n => new PaymentNumberDto
            {
                Id = n.Id,
                Type = n.Type,
                Number = n.Number,
                IsActive = n.IsActive
            }).ToList();

            return Result<IReadOnlyList<PaymentNumberDto>>.Success(dtoList);
        }
    }
}
