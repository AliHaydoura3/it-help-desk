using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Users.UpdateUser;

public sealed class UpdateUserHandler(IIdentityService identityService)
    : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IIdentityService _identityService = identityService;

    public Task<UpdateUserResponse> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        return _identityService.UpdateUserAsync(request, cancellationToken);
    }
}
