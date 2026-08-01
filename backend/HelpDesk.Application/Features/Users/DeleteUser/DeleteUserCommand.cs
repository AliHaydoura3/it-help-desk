using MediatR;

namespace HelpDesk.Application.Features.Users.DeleteUser;

public sealed record DeleteUserCommand(Guid Id, Guid CurrentUserId) : IRequest;
