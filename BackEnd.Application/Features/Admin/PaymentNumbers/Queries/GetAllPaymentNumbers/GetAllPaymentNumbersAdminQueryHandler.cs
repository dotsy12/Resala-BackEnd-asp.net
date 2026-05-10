using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.PaymentNumber;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.PaymentNumbers.Queries.GetAllPaymentNumbers
{
    public class GetAllPaymentNumbersAdminQueryHandler : IRequestHandler<GetAllPaymentNumbersAdminQuery, Result<IReadOnlyList<PaymentNumberDto>>>
    {
        private readonly IPaymentNumberRepository _repository;

        public GetAllPaymentNumbersAdminQueryHandler(IPaymentNumberRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<PaymentNumberDto>>> Handle(GetAllPaymentNumbersAdminQuery request, CancellationToken ct)
        {
            var numbers = await _repository.GetAllAsync(ct);

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
