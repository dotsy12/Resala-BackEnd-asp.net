// GetAllSlotsQuery.cs
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Subscription;
using MediatR;

public record GetAllSlotsQuery : IRequest<Result<IReadOnlyList<AppointmentSlotDto>>>;
