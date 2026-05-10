using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Admin.Staff.Commands.DeleteStaff
{
    public record DeleteStaffCommand(int StaffId) : IRequest<Result<bool>>;
}
