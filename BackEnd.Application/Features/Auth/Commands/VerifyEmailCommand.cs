using BackEnd.Application.Common.ResponseFormat;
using MediatR;


namespace BackEnd.Application.Features.Auth.Commands
{
    public record VerifyEmailCommand(string Email, string Otp)
        : IRequest<Result<string>>;
}