using MediatR;

namespace HelpDesk.Application.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    IEnumerable<string> Roles) : IRequest<UpdateUserResponse>
{
    public Guid Id { get; init; }

    public Guid CurrentUserId { get; init; }
}
