using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Staff;
using MediatR;

namespace BackEnd.Application.Features.Admin.Staff.Queries.GetAllStaff
{
    public record GetAllStaffQuery(
        string? Search = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Result<PagedResult<StaffDto>>>;
}
