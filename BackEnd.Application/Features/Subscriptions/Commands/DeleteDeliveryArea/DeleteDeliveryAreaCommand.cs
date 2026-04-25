using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Commands.DeleteDeliveryArea
{
    public record DeleteDeliveryAreaResponse(int Id, bool Deleted);
    public record DeleteDeliveryAreaCommand(int Id) : IRequest<Result<DeleteDeliveryAreaResponse>>;

    public class DeleteDeliveryAreaHandler : IRequestHandler<DeleteDeliveryAreaCommand, Result<DeleteDeliveryAreaResponse>>
    {
        private readonly IDeliveryAreaRepository _repo;
        private readonly IUnitOfWork _uow;

        public DeleteDeliveryAreaHandler(IDeliveryAreaRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<Result<DeleteDeliveryAreaResponse>> Handle(DeleteDeliveryAreaCommand request, CancellationToken ct)
        {
            var area = await _repo.GetByIdAsync(request.Id, ct);
            if (area == null)
                return Result<DeleteDeliveryAreaResponse>.Failure("منطقة التوصيل غير موجودة.", ErrorType.NotFound);

            _repo.Delete(area);
            await _uow.SaveChangesAsync(ct);

            return Result<DeleteDeliveryAreaResponse>.Success(
                new DeleteDeliveryAreaResponse(request.Id, true), 
                "تم حذف منطقة التوصيل بنجاح.");
        }
    }
}
