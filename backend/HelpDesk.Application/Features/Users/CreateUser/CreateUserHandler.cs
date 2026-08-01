using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Users.CreateUser;

public sealed class CreateUserHandler(IIdentityService identityService)
        : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IIdentityService _identityService = identityService;

    public Task<CreateUserResponse> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        return _identityService.CreateUserAsync(
            request,
            cancellationToken);
    }
}