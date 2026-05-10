using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using MediatR;

namespace BackEnd.Application.Features.Notifications.Commands.RegisterDeviceToken
{
    public record RegisterDeviceTokenCommand(
        int DonorId,
        string Token,
        string? DeviceType) : IRequest<Result<bool>>;

    public class RegisterDeviceTokenHandler : IRequestHandler<RegisterDeviceTokenCommand, Result<bool>>
    {
        private readonly INotificationService _notificationService;

        public RegisterDeviceTokenHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<Result<bool>> Handle(RegisterDeviceTokenCommand request, CancellationToken cancellationToken)
        {
            await _notificationService.RegisterTokenAsync(request.DonorId, request.Token, request.DeviceType);
            return Result<bool>.Success(true);
        }
    }
}
