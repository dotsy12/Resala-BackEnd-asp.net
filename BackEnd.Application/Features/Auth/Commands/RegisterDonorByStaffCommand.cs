using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record RegisterDonorByStaffCommand(
        string Name,
        string Email,
        string PhoneNumber,
        string Password,
        string? Job = null,
        string? Landline = null
    ) : IRequest<Result<RegisterDonorResponse>>;
}