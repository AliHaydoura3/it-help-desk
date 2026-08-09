namespace HelpDesk.Application.Features.Users.CreateUser;

public sealed record CreateUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role
);
