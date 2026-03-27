using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Dtos.InKindDonation;
using MediatR;

namespace BackEnd.Application.Features.InKindDonation.Commands.CreateInKindDonation
{
    public record RecordInKindDonationCommand(
        int DonorId,          
        string DonationTypeName,
        int Quantity,
        string? Description,
        int RecordedByStaffId  
    ) : IRequest<Result<InKindDonationDto>>;
}