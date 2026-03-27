using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Dtos.InKindDonation
{
    // Response DTO
    public record InKindDonationDto(
        int Id,
        int DonorId,
        string DonorName,
        string DonationTypeName,
        int Quantity,
        string? Description,
        int RecordedByStaffId,
        string RecordedByStaffName,
        DateTime RecordedAt,
        DateTime CreatedOn
    );
}
