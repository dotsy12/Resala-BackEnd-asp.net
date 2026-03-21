using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record LoginCommand(
        string? PhoneNumber,
        string? Username,
        string Password
    ) : IRequest<Result<LoginResponse>>;

    public record LoginResponse(
        string Token,
         string RefreshToken,   // ✅ جديد
        string Role,
        int UserId,
        string Name,
        string? PhoneNumber
    );

   
}