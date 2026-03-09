using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record ForgotPasswordCommand(string Email)
        : IRequest<Result<string>>;
}