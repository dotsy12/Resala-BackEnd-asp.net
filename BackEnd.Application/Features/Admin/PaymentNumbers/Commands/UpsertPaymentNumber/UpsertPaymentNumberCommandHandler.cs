using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.PaymentNumber;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.PaymentNumbers.Commands.UpsertPaymentNumber
{
    public class UpsertPaymentNumberCommandHandler : IRequestHandler<UpsertPaymentNumberCommand, Result<PaymentNumberDto>>
    {
        private readonly IPaymentNumberRepository _repository;

        public UpsertPaymentNumberCommandHandler(IPaymentNumberRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<PaymentNumberDto>> Handle(UpsertPaymentNumberCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            // Allow only VodafoneCash and InstaPay
            if (dto.Type != PaymentMethod.VodafoneCash && dto.Type != PaymentMethod.InstaPay)
            {
                return Result<PaymentNumberDto>.Failure("نوع الحساب غير صالح. مسموح فقط بـ VodafoneCash و InstaPay.", ErrorType.BadRequest);
            }

            var existing = await _repository.GetByTypeAsync(dto.Type, ct);

            if (existing != null)
            {
                existing.Update(dto.Number, dto.IsActive);
                _repository.Update(existing);
            }
            else
            {
                existing = PaymentNumber.Create(dto.Type, dto.Number, dto.IsActive);
                await _repository.AddAsync(existing, ct);
            }

            await _repository.SaveChangesAsync(ct);

            var resultDto = new PaymentNumberDto
            {
                Id = existing.Id,
                Type = existing.Type,
                Number = existing.Number,
                IsActive = existing.IsActive
            };

            return Result<PaymentNumberDto>.Success(resultDto);
        }
    }
}
