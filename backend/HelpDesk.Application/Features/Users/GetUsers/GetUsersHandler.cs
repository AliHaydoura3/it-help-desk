using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Users.GetUsers;

public sealed class GetUsersHandler(IIdentityService identityService)
    : IRequestHandler<GetUsersQuery, GetUsersResponse>
{
    private readonly IIdentityService _identityService = identityService;

    public Task<GetUsersResponse> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        return _identityService.GetUsersAsync(
            request,
            cancellationToken);
    }
}
