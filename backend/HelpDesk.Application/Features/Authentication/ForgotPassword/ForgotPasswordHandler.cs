using HelpDesk.Application.Abstractions.Authentication;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpDesk.Application.Features.Authentication.ForgotPassword;

public sealed class ForgotPasswordHandler(
    IIdentityService identityService,
    IPasswordResetEmailSender emailSender,
    ILogger<ForgotPasswordHandler> logger)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var token = await identityService.GeneratePasswordResetTokenAsync(
            request.Email,
            cancellationToken);

        if (token is not null)
        {
            try
            {
                await emailSender.SendAsync(
                    request.Email,
                    token,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Password reset email delivery failed.");
            }
        }
    }
}
