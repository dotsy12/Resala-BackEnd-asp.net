// CreateAppointmentSlotCommand.cs + Handler + Validator
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Commands.CreateAppointmentSlot
{
    public record CreateSlotRequest(
        DateTime SlotDate,
        TimeSpan OpenFrom,
        TimeSpan OpenTo,
        int MaxCapacity,
        string? Notes);

    public record CreateAppointmentSlotCommand(CreateSlotRequest Dto)
        : IRequest<Result<AppointmentSlotDto>>;
}