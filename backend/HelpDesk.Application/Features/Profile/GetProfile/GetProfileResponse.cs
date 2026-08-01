namespace HelpDesk.Application.Features.Profile.GetProfile;

public sealed record GetProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyList<string> Roles);
