// CreateAppointmentSlotCommand.cs + Handler + Validator
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Payment;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackEnd.Application.Features.Subscriptions.Commands.CreateAppointmentSlot
{
    public class CreateAppointmentSlotHandler
        : IRequestHandler<CreateAppointmentSlotCommand, Result<AppointmentSlotDto>>
    {
        private readonly IAppointmentSlotRepository _repo;
        private readonly ILogger<CreateAppointmentSlotHandler> _logger;

        public CreateAppointmentSlotHandler(
            IAppointmentSlotRepository repo,
            ILogger<CreateAppointmentSlotHandler> logger)
        { _repo = repo; _logger = logger; }

        public async Task<Result<AppointmentSlotDto>> Handle(
            CreateAppointmentSlotCommand request, CancellationToken ct)
        {
            var slot = BranchAppointmentSlot.Create(
                slotDate: request.Dto.SlotDate,
                openFrom: request.Dto.OpenFrom,
                openTo: request.Dto.OpenTo,
                maxCapacity: request.Dto.MaxCapacity,
                notes: request.Dto.Notes
            );

            await _repo.AddAsync(slot, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("تم إنشاء Slot: Id={Id} Date={Date}", slot.Id, slot.SlotDate);

            return Result<AppointmentSlotDto>.Success(MapToDto(slot));
        }

        private static AppointmentSlotDto MapToDto(BranchAppointmentSlot s) => new(
            Id: s.Id,
            SlotDate: s.SlotDate,
            OpenFrom: s.OpenFrom.ToString(@"hh\:mm"),
            OpenTo: s.OpenTo.ToString(@"hh\:mm"),
            MaxCapacity: s.MaxCapacity,
            BookedCount: s.BookedCount,
            AvailableSpots: s.AvailableSpots,
            Notes: s.Notes
        );
    }
}