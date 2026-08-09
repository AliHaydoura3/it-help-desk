using MediatR;

namespace HelpDesk.Application.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    string Role) : IRequest<UpdateUserResponse>
{
    public Guid Id { get; init; }

    public Guid CurrentUserId { get; init; }
}
