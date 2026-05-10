using BackEnd.Application.Abstractions.Queries;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.Staff;
using BackEnd.Application.Interfaces.Repositories;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.Staff.Queries.GetAllStaff
{
    public class GetAllStaffQueryHandler : IRequestHandler<GetAllStaffQuery, Result<PagedResult<StaffDto>>>
    {
        private readonly IStaffRepository _staffRepo;

        public GetAllStaffQueryHandler(IStaffRepository staffRepo)
        {
            _staffRepo = staffRepo;
        }

        public async Task<Result<PagedResult<StaffDto>>> Handle(GetAllStaffQuery request, CancellationToken ct)
        {
            var (items, totalCount) = await _staffRepo.GetStaffWithPaginationAsync(
                request.Search, request.PageNumber, request.PageSize, ct);

            var dtoList = items.Select(s => new StaffDto
            {
                Id = s.Id,
                Name = $"{s.FullName.FirstName} {s.FullName.LastName}",
                Username = s.Username,
                Email = s.Email.Value,
                Phone = s.Phone.Value,
                StaffType = s.StaffType.ToString(),
                AccountStatus = s.AccountStatus.ToString(),
                IsActive = s.IsActive,
                CreatedOn = s.CreatedOn
            }).ToList();

            var pagedResult = new PagedResult<StaffDto>(
                dtoList, totalCount, request.PageNumber, request.PageSize);

            return Result<PagedResult<StaffDto>>.Success(pagedResult);
        }
    }
}
