using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Users.DeleteUser;

public sealed class DeleteUserHandler(IIdentityService identityService)
    : IRequestHandler<DeleteUserCommand>
{
    private readonly IIdentityService _identityService = identityService;

    public Task Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        return _identityService.DeleteUserAsync(
            request.Id,
            request.CurrentUserId,
            cancellationToken);
    }
}
