using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Profile.GetProfile;

public sealed class GetProfileHandler(IIdentityService identityService)
    : IRequestHandler<GetProfileQuery, GetProfileResponse>
{
    public Task<GetProfileResponse> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        return identityService.GetProfileAsync(
            request.UserId,
            cancellationToken);
    }
}
