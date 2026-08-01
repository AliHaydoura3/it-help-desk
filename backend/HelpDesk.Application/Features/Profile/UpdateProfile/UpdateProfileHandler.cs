using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Profile.UpdateProfile;

public sealed class UpdateProfileHandler(IIdentityService identityService)
    : IRequestHandler<UpdateProfileCommand, UpdateProfileResponse>
{
    public Task<UpdateProfileResponse> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        return identityService.UpdateProfileAsync(request, cancellationToken);
    }
}
