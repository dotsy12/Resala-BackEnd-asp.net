using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Application.Features.Admin.Staff.Commands.DeleteStaff
{
    public class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand, Result<bool>>
    {
        private readonly IStaffRepository _staffRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DeleteStaffCommandHandler> _logger;

        public DeleteStaffCommandHandler(
            IStaffRepository staffRepo,
            UserManager<ApplicationUser> userManager,
            ILogger<DeleteStaffCommandHandler> logger)
        {
            _staffRepo = staffRepo;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteStaffCommand request, CancellationToken ct)
        {
            _logger.LogInformation("Attempting to soft-delete staff member with Id: {StaffId}", request.StaffId);

            var staff = await _staffRepo.GetByIdWithUserAsync(request.StaffId, ct);
            if (staff == null || staff.IsDeleted)
            {
                _logger.LogWarning("Staff member with Id {StaffId} not found or already deleted.", request.StaffId);
                return Result<bool>.Failure("الموظف غير موجود.", ErrorType.NotFound);
            }

            // 1. Soft delete StaffMember entity
            staff.SetAccountStatus(AccountStatus.Locked);
            // We use reflection or cast if the property is private set in BaseEntity, 
            // but BaseEntity properties are public set in this project structure (checked earlier).
            staff.IsDeleted = true;
            staff.IsActive = false;

            // 2. Soft delete ApplicationUser
            if (staff.User != null)
            {
                staff.User.IsDeleted = true;
                staff.User.IsActive = false;
                
                var updateResult = await _userManager.UpdateAsync(staff.User);
                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to soft-delete Identity User {UserId} for Staff {StaffId}", staff.UserId, staff.Id);
                    return Result<bool>.Failure("فشل تحديث حالة المستخدم في نظام الهوية.", ErrorType.BadRequest);
                }
            }

            await _staffRepo.SaveChangesAsync(ct);

            _logger.LogInformation("Staff member with Id {StaffId} (Username: {Username}) soft-deleted successfully.", staff.Id, staff.Username);

            return Result<bool>.Success(true, "تم حذف الموظف بنجاح (حذف منطقي).");
        }
    }
}
