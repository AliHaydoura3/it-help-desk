namespace HelpDesk.Application.Common.Authentication;

public sealed record UserIdentity(
    Guid Id,
    string Email,
    string Role);
