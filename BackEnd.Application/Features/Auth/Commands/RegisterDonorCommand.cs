using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record RegisterDonorCommand(
        string Name,
        string Email,
        string PhoneNumber,
        string Password,
        string? Job = null,
        string? Landline = null
    ) : IRequest<Result<RegisterDonorResponse>>;

    public record RegisterDonorResponse(int UserId, string Message);

 
}