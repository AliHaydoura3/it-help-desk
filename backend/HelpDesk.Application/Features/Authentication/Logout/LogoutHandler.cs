using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Authentication.Logout;

public sealed class LogoutHandler(IIdentityService identityService)
    : IRequestHandler<LogoutCommand>
{
    public Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        return identityService.RevokeRefreshSessionAsync(
            request.RefreshToken,
            cancellationToken);
    }
}
