using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Features.Authentication.Login;
using MediatR;

namespace HelpDesk.Application.Features.Authentication.RefreshToken;

public sealed class RefreshTokenHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var session = await identityService.RotateRefreshSessionAsync(
            request.RefreshToken,
            cancellationToken);

        if (session is null)
            throw new UnauthorizedAccessException(
                "The refresh token is invalid or has expired.");

        return new LoginResponse(
            jwtTokenGenerator.GenerateAccessToken(session.User),
            session.RefreshToken,
            session.RefreshTokenExpiresAtUtc);
    }
}
