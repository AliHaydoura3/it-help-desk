using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Features.Users.GetUsers;
using MediatR;

namespace HelpDesk.Application.Features.Users.GetUserById;

public sealed class GetUserByIdHandler(IIdentityService identityService)
    : IRequestHandler<GetUserByIdQuery, GetUserResponse>
{
    private readonly IIdentityService _identityService = identityService;

    public Task<GetUserResponse> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        return _identityService.GetUserByIdAsync(
            request.Id,
            cancellationToken);
    }
}
