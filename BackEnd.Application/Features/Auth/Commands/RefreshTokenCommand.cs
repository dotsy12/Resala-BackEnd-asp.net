using BackEnd.Application.Common.ResponseFormat;
using MediatR;

public record RefreshTokenCommand(string RefreshToken)
    : IRequest<Result<RefreshTokenResponse>>;

public record RefreshTokenResponse(string Token, string RefreshToken);