using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record ResetPasswordCommand(
        string Email,
        string Otp,
        string NewPassword
    ) : IRequest<Result<string>>;
}