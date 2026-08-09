namespace HelpDesk.Application.Features.Users.UpdateUser;

public sealed record UpdateUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    string Role);
