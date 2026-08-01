namespace HelpDesk.Application.Common.Authentication;

public sealed record RefreshSession(
    UserIdentity User,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
