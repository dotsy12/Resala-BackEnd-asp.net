// GetAvailableSlotsQuery.cs + Handler
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using MediatR;

namespace BackEnd.Application.Features.Subscriptions.Queries.GetAvailableSlots
{
    public record GetAvailableSlotsQuery : IRequest<Result<IReadOnlyList<AppointmentSlotDto>>>;
}