using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Abstractions.Communication;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Email;

public sealed class SmtpPasswordResetEmailSender(
    IEmailMessageSender emailSender,
    IOptions<SmtpOptions> options) : IPasswordResetEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(
        string email,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        if (!emailSender.IsConfigured)
        {
            throw new InvalidOperationException(
                "SMTP is not configured for password reset emails.");
        }

        var resetUrl = $"{_options.FrontendBaseUrl.TrimEnd('/')}/reset-password" +
            $"?email={Uri.EscapeDataString(email)}" +
            $"&token={Uri.EscapeDataString(resetToken)}";
        await emailSender.SendAsync(
            email,
            "Reset your IT Help Desk password",
            $"Use the following link to reset your password:\n\n{resetUrl}\n\n" +
                "If you did not request this, you can ignore this email.",
            cancellationToken);
    }
}
