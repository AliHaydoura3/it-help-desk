using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Profile.ChangePassword;

public sealed class ChangePasswordHandler(IIdentityService identityService)
    : IRequestHandler<ChangePasswordCommand>
{
    public Task Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        return identityService.ChangePasswordAsync(
            request.UserId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);
    }
}
