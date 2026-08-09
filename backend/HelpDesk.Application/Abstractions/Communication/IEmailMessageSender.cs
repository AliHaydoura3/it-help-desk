namespace HelpDesk.Application.Abstractions.Communication;

public interface IEmailMessageSender
{
    bool IsConfigured { get; }

    Task SendAsync(
        string recipientEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
