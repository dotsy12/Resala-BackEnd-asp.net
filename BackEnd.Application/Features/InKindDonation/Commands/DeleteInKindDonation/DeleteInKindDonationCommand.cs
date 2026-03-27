using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.InKindDonation.Commands.DeleteInKindDonation
{
    public record DeleteInKindDonationCommand(int Id)
    : IRequest<Result<string>>;

}