using System.Net;
using System.Net.Mail;
using HelpDesk.Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Email;

public sealed class SmtpPasswordResetEmailSender(
    IOptions<SmtpOptions> options) : IPasswordResetEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(
        string email,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) ||
            string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new InvalidOperationException(
                "SMTP is not configured for password reset emails.");
        }

        var resetUrl = $"{_options.FrontendBaseUrl.TrimEnd('/')}/reset-password" +
            $"?email={Uri.EscapeDataString(email)}" +
            $"&token={Uri.EscapeDataString(resetToken)}";
        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = "Reset your IT Help Desk password",
            Body = $"Use the following link to reset your password:\n\n{resetUrl}\n\n" +
                "If you did not request this, you can ignore this email.",
            IsBodyHtml = false
        };
        message.To.Add(email);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            client.Credentials = new NetworkCredential(
                _options.Username,
                _options.Password);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
