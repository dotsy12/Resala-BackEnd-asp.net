// GetAvailableSlotsQuery.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsHandler
        : IRequestHandler<GetAvailableSlotsQuery, Result<IReadOnlyList<AppointmentSlotDto>>>
    {
        private readonly IAppointmentSlotRepository _repo;
        public GetAvailableSlotsHandler(IAppointmentSlotRepository repo) => _repo = repo;

        public async Task<Result<IReadOnlyList<AppointmentSlotDto>>> Handle(
            GetAvailableSlotsQuery request, CancellationToken ct)
        {
            var slots = await _repo.GetAvailableAsync(ct);
            var result = slots.Select(s => new AppointmentSlotDto(
                Id: s.Id,
                SlotDate: s.SlotDate,
                OpenFrom: s.OpenFrom.ToString(@"hh\:mm"),
                OpenTo: s.OpenTo.ToString(@"hh\:mm"),
                MaxCapacity: s.MaxCapacity,
                BookedCount: s.BookedCount,
                AvailableSpots: s.AvailableSpots,
                Notes: s.Notes
            )).ToList().AsReadOnly();

            return Result<IReadOnlyList<AppointmentSlotDto>>.Success(result);
        }
    }
}