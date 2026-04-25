using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Commands.UpdateDeliveryArea
{
    public record UpdateDeliveryAreaCommand(
        int Id,
        string Name,
        string Governorate,
        string City,
        bool IsActive
    ) : IRequest<Result<bool>>;

    public class UpdateDeliveryAreaHandler : IRequestHandler<UpdateDeliveryAreaCommand, Result<bool>>
    {
        private readonly IDeliveryAreaRepository _repo;
        private readonly IUnitOfWork _uow;

        public UpdateDeliveryAreaHandler(IDeliveryAreaRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<Result<bool>> Handle(UpdateDeliveryAreaCommand request, CancellationToken ct)
        {
            var area = await _repo.GetByIdAsync(request.Id, ct);
            if (area == null)
                return Result<bool>.Failure("منطقة التوصيل غير موجودة.", ErrorType.NotFound);

            if (await _repo.IsDuplicateAsync(request.Name, request.Governorate, request.City, request.Id, ct))
                return Result<bool>.Failure("هذه البيانات متكررة مع منطقة أخرى.", ErrorType.Conflict);

            area.Update(request.Name, request.Governorate, request.City, request.IsActive);
            
            _repo.Update(area);
            await _uow.SaveChangesAsync(ct);

            return Result<bool>.Success(true, "تم تحديث المنطقة بنجاح.");
        }
    }
}
