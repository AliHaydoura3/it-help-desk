using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Authentication.Login;

public sealed class LoginHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator)
        : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IIdentityService _identityService = identityService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    public async Task<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _identityService.ValidateCredentialsAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var session = await _identityService.CreateRefreshSessionAsync(
            user,
            cancellationToken);

        return new LoginResponse(
            accessToken,
            session.RefreshToken,
            session.RefreshTokenExpiresAtUtc);
    }
}
