namespace HelpDesk.Application.Authentication;

public sealed record UserIdentity(
    Guid Id,
    string Email,
    IEnumerable<string> Roles);