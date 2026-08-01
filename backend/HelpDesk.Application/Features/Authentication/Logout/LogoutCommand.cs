using MediatR;

namespace HelpDesk.Application.Features.Authentication.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;
