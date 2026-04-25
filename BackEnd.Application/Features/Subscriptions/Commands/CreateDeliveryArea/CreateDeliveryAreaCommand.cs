using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Notification;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Commands.CreateDeliveryArea
{
    public record CreateDeliveryAreaResponse(
        int Id,
        string Name,
        string Governorate,
        string City,
        bool IsActive,
        DateTime CreatedAt);

    public record CreateDeliveryAreaCommand(
        string Name,
        string Governorate,
        string City
    ) : IRequest<Result<CreateDeliveryAreaResponse>>;

    public class CreateDeliveryAreaHandler : IRequestHandler<CreateDeliveryAreaCommand, Result<CreateDeliveryAreaResponse>>
    {
        private readonly IDeliveryAreaRepository _repo;
        private readonly IUnitOfWork _uow;

        public CreateDeliveryAreaHandler(IDeliveryAreaRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<Result<CreateDeliveryAreaResponse>> Handle(CreateDeliveryAreaCommand request, CancellationToken ct)
        {
            if (await _repo.IsDuplicateAsync(request.Name, request.Governorate, request.City, null, ct))
                return Result<CreateDeliveryAreaResponse>.Failure("هذه المنطقة مسجلة بالفعل في نفس المحافظة والمدينة.", ErrorType.Conflict);

            var area = DeliveryArea.Create(request.Name, request.Governorate, request.City);
            
            await _repo.AddAsync(area, ct);
            await _uow.SaveChangesAsync(ct);

            var response = new CreateDeliveryAreaResponse(
                area.Id,
                area.Name,
                area.Governorate,
                area.City,
                area.IsActive,
                area.CreatedOn
            );

            return Result<CreateDeliveryAreaResponse>.Success(response, "تم إنشاء منطقة التوصيل بنجاح.");
        }
    }
}
