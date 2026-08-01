namespace HelpDesk.Application.Abstractions.Authentication;

public interface IPasswordResetEmailSender
{
    Task SendAsync(
        string email,
        string resetToken,
        CancellationToken cancellationToken = default);
}
