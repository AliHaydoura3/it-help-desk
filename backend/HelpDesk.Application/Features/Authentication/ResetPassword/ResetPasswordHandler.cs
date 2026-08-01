using HelpDesk.Application.Abstractions.Authentication;
using MediatR;

namespace HelpDesk.Application.Features.Authentication.ResetPassword;

public sealed class ResetPasswordHandler(IIdentityService identityService)
    : IRequestHandler<ResetPasswordCommand>
{
    public Task Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        return identityService.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword,
            cancellationToken);
    }
}
