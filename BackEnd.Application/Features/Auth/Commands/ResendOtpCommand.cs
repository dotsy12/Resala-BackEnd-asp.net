using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record ResendOtpCommand(string Email, string OtpType)
        : IRequest<Result<string>>;
    // OtpType: "EmailVerification" أو "PasswordReset"
}