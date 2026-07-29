using MediatR;

namespace HelpDesk.Application.Features.Authentication.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse>;