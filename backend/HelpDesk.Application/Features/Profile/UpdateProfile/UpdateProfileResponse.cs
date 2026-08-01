namespace HelpDesk.Application.Features.Profile.UpdateProfile;

public sealed record UpdateProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyList<string> Roles);
