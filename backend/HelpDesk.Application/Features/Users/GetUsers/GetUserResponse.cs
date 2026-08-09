namespace HelpDesk.Application.Features.Users.GetUsers;

public sealed record GetUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    string Role);
