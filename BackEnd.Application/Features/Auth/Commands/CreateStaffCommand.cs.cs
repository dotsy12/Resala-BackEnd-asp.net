using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record CreateStaffCommand(
        string Name,
        string Username,
        string Email,
        string PhoneNumber,
        string Password,
        StaffType StaffType
    ) : IRequest<Result<CreateStaffResponse>>;

    public record CreateStaffResponse(int StaffId, string Username);
}