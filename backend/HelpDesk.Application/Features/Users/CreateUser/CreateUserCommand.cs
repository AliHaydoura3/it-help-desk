using MediatR;

namespace HelpDesk.Application.Features.Users.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role
) : IRequest<CreateUserResponse>;
