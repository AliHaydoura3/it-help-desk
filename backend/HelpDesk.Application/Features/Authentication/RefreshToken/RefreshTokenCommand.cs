using MediatR;

namespace HelpDesk.Application.Features.Authentication.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken)
    : IRequest<Login.LoginResponse>;
